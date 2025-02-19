﻿// OpenTween - Client of Twitter
// Copyright (c) 2007-2011 kiri_feather (@kiri_feather) <kiri.feather@gmail.com>
//           (c) 2008-2011 Moz (@syo68k)
//           (c) 2008-2011 takeshik (@takeshik) <http://www.takeshik.org/>
//           (c) 2010-2011 anis774 (@anis774) <http://d.hatena.ne.jp/anis774/>
//           (c) 2010-2011 fantasticswallow (@f_swallow) <http://twitter.com/f_swallow>
//           (c) 2011      Egtra (@egtra) <http://dev.activebasic.com/egtra/>
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
using System.Net;
using OpenTween.Api.DataModel;
using OpenTween.Models;
using OpenTween.SocialProtocol.Twitter;

namespace OpenTween
{
    public class UserInfo
    {
        public UserInfo()
        {
        }

        public UserInfo(TwitterUser user)
        {
            this.Id = new TwitterUserId(user.IdStr);
            this.Name = WebUtility.HtmlDecode(user.Name).Trim();
            this.ScreenName = user.ScreenName;
            this.Location = WebUtility.HtmlDecode(user.Location);
            this.Description = WebUtility.HtmlDecode(user.Description);
            this.ImageUrl = user.ProfileImageUrlHttps is { } imageUrlStr
                ? new TwitterProfileImageUri(imageUrlStr)
                : null;
            this.Url = user.Url ?? "";
            this.Protect = user.Protected;
            this.FriendsCount = user.FriendsCount;
            this.FollowersCount = user.FollowersCount;
            this.CreatedAt = MyCommon.DateTimeParse(user.CreatedAt);
            this.StatusesCount = user.StatusesCount;
            this.Verified = user.Verified;
            if (user.Status != null)
            {
                this.RecentPost = user.Status.FullText;
                this.PostCreatedAt = MyCommon.DateTimeParse(user.Status.CreatedAt);
                this.PostSource = user.Status.Source;
            }
        }

        public PersonId Id = null!;
        public string Name = "";
        public string ScreenName = "";
        public string Location = "";
        public string Description = "";
        public IResponsiveImageUri? ImageUrl = null;
        public string Url = "";
        public bool Protect = false;
        public int FriendsCount = 0;
        public int FollowersCount = 0;
        public int FavoriteCount = 0;
        public DateTimeUtc CreatedAt;
        public int StatusesCount = 0;
        public bool Verified = false;
        public string RecentPost = "";
        public DateTimeUtc PostCreatedAt;
        public string PostSource = "";        // html形式　"<a href="http://sourceforge.jp/projects/tween/wiki/FrontPage" rel="nofollow">Tween</a>"

        public override string ToString()
            => this.ScreenName + " / " + this.Name;
    }
}
