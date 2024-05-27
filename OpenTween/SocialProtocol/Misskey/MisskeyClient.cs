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
using OpenTween.Api.Misskey;
using OpenTween.Models;

namespace OpenTween.SocialProtocol.Misskey
{
    public class MisskeyClient : ISocialProtocolClient
    {
        private readonly MisskeyAccount account;
        private readonly MisskeyPostFactory postFactory = new();

        public MisskeyClient(MisskeyAccount account)
            => this.account = account;

        public async Task<UserInfo> VerifyCredentials()
        {
            var request = new MeRequest();
            var user = await request.Send(this.account.Connection)
                .ConfigureAwait(false);

            this.account.AccountState.UpdateFromUser(user);

            return new();
        }

        public async Task<PostClass> GetPostById(PostId postId, bool firstLoad)
        {
            var request = new NoteShowRequest
            {
                NoteId = this.AssertMisskeyNoteId(postId),
            };

            var note = await request.Send(this.account.Connection)
                .ConfigureAwait(false);

            var post = this.CreatePostFromNote(note, firstLoad);

            return post;
        }

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

        private MisskeyNoteId AssertMisskeyNoteId(PostId postId)
        {
            return postId is MisskeyNoteId noteId
                ? noteId
                : throw new WebApiException($"Not supported type: {postId.GetType()}");
        }

        private PostClass CreatePostFromNote(MisskeyNote note, bool firstLoad)
            => this.postFactory.CreateFromNote(note, this.account.AccountState, firstLoad);
    }
}
