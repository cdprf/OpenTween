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

using System;
using Xunit;

namespace OpenTween.SocialProtocol.Twitter
{
    public class TwitterProfileImageUriTest
    {
        [Theory]
        [InlineData(24, "mini")]
        [InlineData(25, "normal")]
        [InlineData(48, "normal")]
        [InlineData(49, "bigger")]
        [InlineData(73, "bigger")]
        [InlineData(74, "original")]
        public void SizeName_GetPreferredSize_Test(int sizePx, string expected)
        {
            var size = TwitterProfileImageUri.SizeName.GetPreferredSize(sizePx);
            Assert.Equal(expected, size.Name);
        }

        [Fact]
        public void SizeName_GetLargerOrSameSize_Test()
        {
            var expected = new[]
            {
                TwitterProfileImageUri.SizeName.Normal,
                TwitterProfileImageUri.SizeName.Bigger,
                TwitterProfileImageUri.SizeName.Original,
            };
            Assert.Equal(expected, TwitterProfileImageUri.SizeName.GetLargerOrSameSize(minSizePx: 48));
        }

        [Theory]
        [InlineData("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg", 48, "https://pbs.twimg.com/profile_images/00000/foo_normal.jpg")]
        [InlineData("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg", 73, "https://pbs.twimg.com/profile_images/00000/foo_bigger.jpg")]
        [InlineData("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg", 24, "https://pbs.twimg.com/profile_images/00000/foo_mini.jpg")]
        [InlineData("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg", 100, "https://pbs.twimg.com/profile_images/00000/foo.jpg")]
        [InlineData("https://pbs.twimg.com/profile_images/00000/foo_normal_bar_normal.jpg", 100, "https://pbs.twimg.com/profile_images/00000/foo_normal_bar.jpg")]
        public void GetImageUri_Test(string normalUrl, int sizePx, string expected)
        {
            var responsiveImageUri = new TwitterProfileImageUri(normalUrl);
            Assert.Equal(expected, responsiveImageUri.GetImageUri(sizePx).AbsoluteUri);
        }

        [Fact]
        public void GetImageUriLargerOrSameSize_Test()
        {
            var responsiveImageUri = new TwitterProfileImageUri("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg");
            var expected = new Uri[]
            {
                new("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg"),
                new("https://pbs.twimg.com/profile_images/00000/foo_bigger.jpg"),
                new("https://pbs.twimg.com/profile_images/00000/foo.jpg"),
            };
            Assert.Equal(expected, responsiveImageUri.GetImageUriLargerOrSameSize(minSizePx: 48));
        }

        [Fact]
        public void GetOriginalImageUri_Test()
        {
            var responsiveImageUri = new TwitterProfileImageUri("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg");
            Assert.Equal(new("https://pbs.twimg.com/profile_images/00000/foo.jpg"), responsiveImageUri.GetOriginalImageUri());
        }

        [Fact]
        public void GetFileName_Test()
        {
            var responsiveImageUri = new TwitterProfileImageUri("https://pbs.twimg.com/profile_images/00000/foo_normal.jpg");
            Assert.Equal("foo.jpg", responsiveImageUri.GetFilename());
        }
    }
}
