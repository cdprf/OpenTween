// OpenTween - Client of Twitter
// Copyright (c) 2007-2011 kiri_feather (@kiri_feather) <kiri.feather@gmail.com>
//           (c) 2008-2011 Moz (@syo68k)
//           (c) 2008-2011 takeshik (@takeshik) <http://www.takeshik.org/>
//           (c) 2010-2011 anis774 (@anis774) <http://d.hatena.ne.jp/anis774/>
//           (c) 2010-2011 fantasticswallow (@f_swallow) <http://twitter.com/f_swallow>
//           (c) 2011      Egtra (@egtra) <http://dev.activebasic.com/egtra/>
//           (c) 2013      kim_upsilon (@kim_upsilon) <https://upsilo.net/~upsilon/>
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

using System.Linq;
using System.Threading.Tasks;
using OpenTween.Api.GraphQL;
using OpenTween.Models;

namespace OpenTween.SocialProtocol.Twitter
{
    public class TwitterGraphqlClient : ISocialProtocolClient
    {
        private readonly TwitterAccount account;

        public TwitterGraphqlClient(TwitterAccount account)
        {
            this.account = account;
        }

        public async Task<PostClass> GetPostById(PostId postId, bool firstLoad)
        {
            this.account.Legacy.CheckAccountState();

            var statusId = this.AssertTwitterStatusId(postId);
            var request = new TweetDetailRequest
            {
                FocalTweetId = statusId,
            };

            var tweets = await request.Send(this.account.Connection)
                .ConfigureAwait(false);

            var status = tweets.Select(x => x.ToTwitterStatus())
                .Where(x => x.IdStr == statusId.Id)
                .FirstOrDefault() ?? throw new WebApiException("Empty result set");

            var post = this.account.Legacy.CreatePostsFromStatusData(status, firstLoad, favTweet: false);

            return post;
        }

        public async Task<TimelineResponse> GetHomeTimeline(int count, IQueryCursor? cursor, bool firstLoad)
        {
            this.account.Legacy.CheckAccountState();

            var request = new HomeLatestTimelineRequest
            {
                Count = count,
                Cursor = cursor?.As<TwitterGraphqlCursor>(),
            };

            var response = await request.Send(this.account.Connection)
                .ConfigureAwait(false);

            var statuses = response.ToTwitterStatuses();
            var cursorTop = response.CursorTop;
            var cursorBottom = response.CursorBottom;

            var posts = this.account.Legacy.CreatePostsFromJson(statuses, firstLoad);
            posts = this.account.Legacy.FilterNoRetweetUserPosts(posts);

            return new(posts, cursorTop, cursorBottom);
        }

        public async Task<TimelineResponse> GetSearchTimeline(string query, string lang, int count, IQueryCursor? cursor, bool firstLoad)
        {
            this.account.Legacy.CheckAccountState();

            if (!MyCommon.IsNullOrEmpty(lang))
                query = $"({query}) lang:{lang}";

            var request = new SearchTimelineRequest(query)
            {
                Count = count,
                Cursor = cursor?.As<TwitterGraphqlCursor>(),
            };

            var response = await request.Send(this.account.Connection)
                .ConfigureAwait(false);

            var statuses = response.ToTwitterStatuses();
            var cursorTop = response.CursorTop;
            var cursorBottom = response.CursorBottom;

            var posts = this.account.Legacy.CreatePostsFromJson(statuses, firstLoad);
            posts = this.account.Legacy.FilterNoRetweetUserPosts(posts);

            return new(posts, cursorTop, cursorBottom);
        }

        public async Task DeletePost(PostId postId)
        {
            var statusId = this.AssertTwitterStatusId(postId);
            var request = new DeleteTweetRequest
            {
                TweetId = statusId,
            };

            await request.Send(this.account.Connection)
                .ConfigureAwait(false);
        }

        public async Task FavoritePost(PostId postId)
        {
            var statusId = this.AssertTwitterStatusId(postId);
            var request = new FavoriteTweetRequest
            {
                TweetId = statusId,
            };

            await request.Send(this.account.Connection)
                .ConfigureAwait(false);
        }

        public async Task UnfavoritePost(PostId postId)
        {
            var statusId = this.AssertTwitterStatusId(postId);
            var request = new UnfavoriteTweetRequest
            {
                TweetId = statusId,
            };

            await request.Send(this.account.Connection)
                .ConfigureAwait(false);
        }

        public async Task<PostClass?> RetweetPost(PostId postId)
        {
            var statusId = this.AssertTwitterStatusId(postId);
            var request = new CreateRetweetRequest
            {
                TweetId = statusId,
            };

            await request.Send(this.account.Connection)
                .ConfigureAwait(false);

            // graphql のレスポンスには PostClass 生成に必要な情報が不足しているため null を返す
            return null;
        }

        public async Task UnretweetPost(PostId postId)
        {
            var statusId = this.AssertTwitterStatusId(postId);
            var request = new DeleteRetweetRequest
            {
                SourceTweetId = statusId,
            };

            await request.Send(this.account.Connection)
                .ConfigureAwait(false);
        }

        private TwitterStatusId AssertTwitterStatusId(PostId postId)
        {
            return postId is TwitterStatusId statusId
                ? statusId
                : throw new WebApiException($"Not supported type: {postId.GetType()}");
        }
    }
}
