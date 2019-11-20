namespace LightStreamWeb.Frontend {
    export interface GetPostsOptions {
        limit: number;
        rssUrl: string;
    }

    interface Post {
        Title: string;
        Summary: string;
        ImageUrl: string;
        BlogPostUrl: string;
    }

    interface GetPostResponse {
        posts: Post[];
    }

    export class BlogService {
        constructor(private _$http: angular.IHttpService) {
        }

        public getPosts(options: GetPostsOptions): angular.IHttpPromise<GetPostResponse> {
            options.limit = options.limit || 0; //defaulting to 0 returns all blog posts

            return this._$http({
                method: 'GET',
                url: '/api/marketing/blog',
                params: {
                    rssUrl: options.rssUrl,
                    limit: options.limit
                }
            });
        }
    }

    angular.module('LightStreamApp').service('BlogService', ['$http', BlogService]);
}