// OpenTween - Client of Twitter
// Copyright (c) 2024 kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
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
using System.Diagnostics.CodeAnalysis;
using System.Net.Cache;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace OpenTween.Connection
{
    public sealed class MisskeyApiConnection : IApiConnection, IDisposable
    {
        public bool IsDisposed { get; private set; } = false;

        internal HttpClient Http;

        private readonly Uri apiBaseUri;
        private readonly string accessToken;

        public MisskeyApiConnection(Uri apiBaseUri, string accessToken)
        {
            this.apiBaseUri = apiBaseUri;
            this.accessToken = accessToken;

            this.InitializeHttpClients();
            Networking.WebProxyChanged += this.Networking_WebProxyChanged;
        }

        [MemberNotNull(nameof(Http))]
        private void InitializeHttpClients()
        {
            this.Http = InitializeHttpClient();

            // タイムアウト設定は IHttpRequest.Timeout でリクエスト毎に制御する
            this.Http.Timeout = Timeout.InfiniteTimeSpan;
        }

        public Task<ApiResponse> SendAsync(IHttpRequest request)
            => throw new WebApiException("Not implemented");

        public void Dispose()
        {
            if (this.IsDisposed)
                return;

            Networking.WebProxyChanged -= this.Networking_WebProxyChanged;
            this.Http.Dispose();

            this.IsDisposed = true;
        }

        private void Networking_WebProxyChanged(object sender, EventArgs e)
            => this.InitializeHttpClients();

        private static HttpClient InitializeHttpClient()
        {
            var builder = Networking.CreateHttpClientBuilder();

            builder.SetupHttpClientHandler(
                x => x.CachePolicy = new RequestCachePolicy(RequestCacheLevel.BypassCache)
            );

            return builder.Build();
        }
    }
}
