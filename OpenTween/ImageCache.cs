﻿// OpenTween - Client of Twitter
// Copyright (c) 2013 kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
// All rights reserved.
//
// This file is part of OpenTween.
//
// This program is free software; you can redistribute it and/or modify it
// under the terms of the GNU General Public License as published by the Free
// Software Foundation; either version 3 of the License, or (at your option)
// any later version.
//
// This program is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License
// for more details.
//
// You should have received a copy of the GNU General Public License along
// with this program. If not, see <http://www.gnu.org/licenses/>, or write to
// the Free Software Foundation, Inc., 51 Franklin Street - Fifth Floor,
// Boston, MA 02110-1301, USA.

#nullable enable

using System;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using OpenTween.Connection;
using OpenTween.Models;
using OpenTween.SocialProtocol.Twitter;

namespace OpenTween
{
    public class ImageCache : IDisposable
    {
        /// <summary>
        /// キャッシュとして URL と取得した画像を対に保持する辞書
        /// </summary>
        internal LRUCacheDictionary<string, Task<MemoryImage>> InnerDictionary;

        /// <summary>
        /// 非同期タスクをキャンセルするためのトークンのもと
        /// </summary>
        private CancellationTokenSource cancelTokenSource;

        /// <summary>
        /// オブジェクトが破棄された否か
        /// </summary>
        private bool disposed = false;

        public ImageCache()
        {
            this.InnerDictionary = new LRUCacheDictionary<string, Task<MemoryImage>>(trimLimit: 300, autoTrimCount: 100);
            this.InnerDictionary.CacheRemoved += (s, e) =>
            {
                this.CacheRemoveCount++;

                // まだ参照されている場合もあるのでDisposeはファイナライザ任せ
                var task = e.Item.Value;
                _ = AsyncExceptionBoundary.IgnoreException(task);
            };

            this.cancelTokenSource = new CancellationTokenSource();
        }

        /// <summary>
        /// 保持しているキャッシュの件数
        /// </summary>
        public long CacheCount
            => this.InnerDictionary.Count;

        /// <summary>
        /// 破棄されたキャッシュの件数
        /// </summary>
        public int CacheRemoveCount { get; private set; }

        /// <summary>
        /// 指定された URL にある画像を非同期に取得するメソッド
        /// </summary>
        /// <param name="address">取得先の URL</param>
        /// <param name="force">キャッシュを使用せずに取得する場合は true</param>
        /// <returns>非同期に画像を取得するタスク</returns>
        public Task<MemoryImage> DownloadImageAsync(Uri address, bool force = false)
        {
            var cancelToken = this.cancelTokenSource.Token;
            var addressStr = address.AbsoluteUri;

            this.InnerDictionary.TryGetValue(addressStr, out var cachedImageTask);

            if (cachedImageTask != null && !force)
                return cachedImageTask;

            cancelToken.ThrowIfCancellationRequested();

            var imageTask = Task.Run(() => this.FetchImageAsync(address, cancelToken));
            this.InnerDictionary[addressStr] = imageTask;

            return imageTask;
        }

        private async Task<MemoryImage> FetchImageAsync(Uri uri, CancellationToken cancelToken)
        {
            try
            {
                using var response = await Networking.Http.GetAsync(uri, cancelToken)
                    .ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                using var imageStream = await response.Content.ReadAsStreamAsync()
                    .ConfigureAwait(false);

                return await MemoryImage.CopyFromStreamAsync(imageStream)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
                when (ex is HttpRequestException or OperationCanceledException or InvalidImageException)
            {
                return this.CreateBlankImage();
            }
        }

        private MemoryImage CreateBlankImage()
        {
            using var bitmap = new Bitmap(1, 1);
            return MemoryImage.CopyFromImage(bitmap);
        }

        public MemoryImage? TryGetFromCache(Uri address)
        {
            var addressStr = address.AbsoluteUri;

            if (!this.InnerDictionary.TryGetValue(addressStr, out var imageTask) ||
                imageTask.Status != TaskStatus.RanToCompletion)
                return null;

            return imageTask.Result;
        }

        public MemoryImage? TryGetLargerOrSameSizeFromCache(IResponsiveImageUri responseImageUri, int minSizePx)
        {
            var imageUris = responseImageUri.GetImageUriLargerOrSameSize(minSizePx);

            foreach (var imageUri in imageUris)
            {
                var image = this.TryGetFromCache(imageUri);
                if (image != null)
                    return image;
            }

            return null;
        }

        public void CancelAsync()
        {
            var oldTokenSource = this.cancelTokenSource;
            this.cancelTokenSource = new CancellationTokenSource();

            oldTokenSource.Cancel();
            oldTokenSource.Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this.disposed) return;

            if (disposing)
            {
                this.CancelAsync();

                foreach (var (_, task) in this.InnerDictionary)
                {
                    if (task.Status == TaskStatus.RanToCompletion)
                        task.Result?.Dispose();
                }

                this.InnerDictionary.Clear();
                this.cancelTokenSource.Dispose();
            }

            this.disposed = true;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~ImageCache()
            => this.Dispose(false);
    }
}
