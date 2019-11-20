var LightStreamWeb;
(function (LightStreamWeb) {
    var Frontend;
    (function (Frontend) {
        var BlogService = /** @class */ (function () {
            function BlogService(_$http) {
                this._$http = _$http;
            }
            BlogService.prototype.getPosts = function (options) {
                options.limit = options.limit || 0; //defaulting to 0 returns all blog posts
                return this._$http({
                    method: 'GET',
                    url: '/api/marketing/blog',
                    params: {
                        rssUrl: options.rssUrl,
                        limit: options.limit
                    }
                });
            };
            return BlogService;
        }());
        Frontend.BlogService = BlogService;
        angular.module('LightStreamApp').service('BlogService', ['$http', BlogService]);
    })(Frontend = LightStreamWeb.Frontend || (LightStreamWeb.Frontend = {}));
})(LightStreamWeb || (LightStreamWeb = {}));
//# sourceMappingURL=blog-service.js.map