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
using System.IO;
using System.Linq;
using OpenTween.Models;

namespace OpenTween.SocialProtocol.Twitter
{
    public record TwitterProfileImageUri(
        string NormalImageUrlStr
    ) : IResponsiveImageUri
    {
        public record SizeName
        {
            public string Name { get; init; }

            private SizeName(string name)
                => this.Name = name;

            public static readonly SizeName Mini = new("mini");

            public static readonly SizeName Normal = new("normal");

            public static readonly SizeName Bigger = new("bigger");

            public static readonly SizeName Original = new("original");

            private static readonly (SizeName Size, int MaxSizePx)[] SizeNames = new[]
            {
                (Mini, 24),
                (Normal, 48),
                (Bigger, 73),
                (Original, int.MaxValue),
            };

            public static SizeName GetPreferredSize(int sizePx)
                => SizeNames.Where(x => sizePx <= x.MaxSizePx).First().Size;

            public static SizeName[] GetLargerOrSameSize(int minSizePx)
                => SizeNames.Where(x => minSizePx <= x.MaxSizePx).Select(x => x.Size).ToArray();
        }

        public Uri GetImageUri(int sizePx)
        {
            var sizeName = SizeName.GetPreferredSize(sizePx);

            return this.GetImageUri(sizeName);
        }

        public Uri GetImageUri(SizeName size)
        {
            var normalUrlStr = this.NormalImageUrlStr;

            // see: https://developer.twitter.com/en/docs/twitter-api/v1/accounts-and-users/user-profile-images-and-banners
            string imageUrlStr;
            if (size == SizeName.Normal)
                imageUrlStr = normalUrlStr;
            else if (size == SizeName.Original)
                imageUrlStr = normalUrlStr.Replace("_normal.", ".");
            else
                imageUrlStr = normalUrlStr.Replace("_normal.", $"_{size.Name}.");

            return new(imageUrlStr);
        }

        public Uri[] GetImageUriLargerOrSameSize(int minSizePx)
        {
            var sizes = SizeName.GetLargerOrSameSize(minSizePx);

            return sizes.Select(x => this.GetImageUri(x)).ToArray();
        }

        public Uri GetOriginalImageUri()
            => this.GetImageUri(SizeName.Original);

        public string GetFilename()
            => Path.GetFileName(this.GetOriginalImageUri().AbsolutePath);
    }
}
