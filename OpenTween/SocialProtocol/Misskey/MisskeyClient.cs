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

using System.Threading.Tasks;
using OpenTween.Models;

namespace OpenTween.SocialProtocol.Misskey
{
    public class MisskeyClient : ISocialProtocolClient
    {
        public Task<UserInfo> VerifyCredentials()
            => throw this.CreateException();

        public Task<PostClass> GetPostById(PostId postId, bool firstLoad)
            => throw this.CreateException();

        public Task<TimelineResponse> GetHomeTimeline(int count, IQueryCursor? cursor, bool firstLoad)
            => throw this.CreateException();

        public Task<TimelineResponse> GetMentionsTimeline(int count, IQueryCursor? cursor, bool firstLoad)
            => throw this.CreateException();

        public Task<TimelineResponse> GetFavoritesTimeline(int count, IQueryCursor? cursor, bool firstLoad)
            => throw this.CreateException();

        public Task<TimelineResponse> GetListTimeline(long listId, int count, IQueryCursor? cursor, bool firstLoad)
            => throw this.CreateException();

        public Task<TimelineResponse> GetSearchTimeline(string query, string lang, int count, IQueryCursor? cursor, bool firstLoad)
            => throw this.CreateException();

        public Task<PostClass[]> GetRelatedPosts(PostClass targetPost, bool firstLoad)
            => throw this.CreateException();

        public Task<PostClass?> CreatePost(PostStatusParams postParams)
            => throw this.CreateException();

        public int GetTextLengthRemain(PostStatusParams postParams)
            => 0;

        public Task DeletePost(PostId postId)
            => throw this.CreateException();

        public Task FavoritePost(PostId postId)
            => throw this.CreateException();

        public Task UnfavoritePost(PostId postId)
            => throw this.CreateException();

        public Task<PostClass?> RetweetPost(PostId postId)
            => throw this.CreateException();

        public Task UnretweetPost(PostId postId)
            => throw this.CreateException();

        public Task RefreshConfiguration()
            => Task.CompletedTask;

        private WebApiException CreateException()
            => new("Not implemented");
    }
}
