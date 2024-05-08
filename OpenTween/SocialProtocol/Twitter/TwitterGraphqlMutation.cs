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
using OpenTween.Api.GraphQL;
using OpenTween.Models;

namespace OpenTween.SocialProtocol.Twitter
{
    public class TwitterGraphqlMutation : ISocialProtocolMutation
    {
        private readonly TwitterAccount account;

        public TwitterGraphqlMutation(TwitterAccount account)
        {
            this.account = account;
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

        private TwitterStatusId AssertTwitterStatusId(PostId postId)
        {
            return postId is TwitterStatusId statusId
                ? statusId
                : throw new WebApiException($"Not supported type: {postId.GetType()}");
        }
    }
}
