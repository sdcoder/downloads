(function ($) {
    $.VideoTracking = function (options) {
        options = options || {};
        options.isInDebugMode = options.isInDebugMode === undefined ? false : options.isInDebugMode;

        var self = this;
        self.clickSource = undefined;

        /*a more friendlier way to keep track of what the hell event42, prop34, blah, blah, blah mean*/
        var _trackingVars = {
            events: {
                videoStarted: { name: 'event42', description: 'Video Starts (e42)' },
                videoCompleted: { name: 'event43', description: 'Video Completes  (e42)' },
                video25PercentPlayed: { name: 'event45', description: 'Video 25% (e45)' },
                video50PercentPlayed: { name: 'event46', description: 'Video 50% (e46)' },
                video75PercentPlayed: { name: 'event47', description: 'Video 75% (e47)' },
            },
            properties: {
                videoTitleOrName: { name: 'prop34', description: 'Video Title/Name (p34)', value: 'undefined' }
            },
            variables: {
                videoTitleOrName: { name: 'eVar41', description: 'Video Title/Name (v41)', value: 'undefined' }
            }
        };

        /*Lists of dfds to keep track of our custom events: video starts, completes, 25%, 50%, 75% watched*/
        var _dfds = {
            videoStarted: $.Deferred(),
            videoCompleted: $.Deferred(),
            video25PercentWatched: $.Deferred(),
            video50PercentWatched: $.Deferred(),
            video75PercentWatched: $.Deferred(),
        };

        var _console = {
            log: function (thingyToLog) {
                if (options.isInDebugMode && console && JSON && JSON !== undefined) {
                    console.log(JSON.stringify(thingyToLog));
                    console.log(thingyToLog);
                }
            }
        }

        self.setVideoTitle = function (title) {
            _trackingVars.properties.videoTitleOrName.value = title;
            _trackingVars.variables.videoTitleOrName.value = title;
        };

        self.on = function (event, callback) {
            $(self).on(event, callback);
        };

        self.videoStarted = function () {
            var promise = _dfds.videoStarted.promise();

            promise.done(function (data) {
                _console.log(data);
            });

            return promise;
        };

        self.videoCompleted = function () {
            var promise = _dfds.videoCompleted.promise();

            promise.done(function (data) {
                _console.log(data);
            });

            return promise;
        };

        self.video25PercentWatched = function () {
            var promise = _dfds.video25PercentWatched.promise();

            promise.done(function (data) {
                _console.log(data);
            });

            return promise;
        };

        self.video50PercentWatched = function () {
            var promise = _dfds.video50PercentWatched.promise();

            promise.done(function (data) {
                _console.log(data);
            });

            return promise;
        };

        self.video75PercentWatched = function () {
            return _dfds.video75PercentWatched.promise();

            promise.done(function (data) {
                _console.log(data);
            });

            return promise;
        };

        self.onVideoPlayed = function (event, data) {
            if (event && !event.type)//if data isn't coming from the mock player
                data = event;

            if (_dfds.videoStarted.state() === 'pending')
                _dfds.videoStarted.resolve({
                    event: _trackingVars.events.videoStarted,
                    property: _trackingVars.properties.videoTitleOrName,
                    variable: _trackingVars.variables.videoTitleOrName,
                    video: data
                });
        };

        self.onVideoEnded = function (event, data) {
            if (event && !event.type)
                data = event;

            if (_dfds.videoCompleted.state() === 'pending')
                _dfds.videoCompleted.resolve({
                    event: _trackingVars.events.videoCompleted,
                    property: _trackingVars.properties.videoTitleOrName,
                    variable: _trackingVars.variables.videoTitleOrName,
                    video: data
                });
        };

        self.onTimeUpdated = function (event, data) {
            if (event && !event.type)
                data = event;

            if (data.percent > .25 && _dfds.video25PercentWatched.state() === 'pending')
                _dfds.video25PercentWatched.resolve({
                    event: _trackingVars.events.video25PercentPlayed,
                    property: _trackingVars.properties.videoTitleOrName,
                    variable: _trackingVars.variables.videoTitleOrName,
                    video: data
                });

            if (data.percent > .50 && _dfds.video50PercentWatched.state() === 'pending')
                _dfds.video50PercentWatched.resolve({
                    event: _trackingVars.events.video50PercentPlayed,
                    property: _trackingVars.properties.videoTitleOrName,
                    variable: _trackingVars.variables.videoTitleOrName,
                    video: data
                });

            if (data.percent > .75 && _dfds.video75PercentWatched.state() === 'pending')
                _dfds.video75PercentWatched.resolve({
                    event: _trackingVars.events.video75PercentPlayed,
                    property: _trackingVars.properties.videoTitleOrName,
                    variable: _trackingVars.variables.videoTitleOrName,
                    video: data
                });
        };

        self.reset = function () {
            _dfds = {
                videoStarted: $.Deferred(),
                videoCompleted: $.Deferred(),
                video25PercentWatched: $.Deferred(),
                video50PercentWatched: $.Deferred(),
                video75PercentWatched: $.Deferred(),
            };
        }
    };

    $.LsYoutTubeVideo = function (options) {
        var self = this;

        var _videoTracking = new $.VideoTracking();

        var _id;
        var _iframeContainer;
        var _interval;
        var _isIOS = navigator.userAgent.match(/(iPad)|(iPhone)|(iPod)/i) != null;

        var _onPlayerReady = function (event) {
            
            _videoTracking.setVideoTitle(self.videoPlayer.getVideoData().title);

            if (self.onPlayerReady)
                self.onPlayerReady(self.videoPlayer, _isIOS);
        };

        var _onIntervalFired = function (event) {
            var data = {
                duration: self.videoPlayer.getDuration(),
                seconds: self.videoPlayer.getCurrentTime()
            };


            data.percent = data.duration == 0 ? 0 : data.seconds / data.duration;

            _videoTracking.onTimeUpdated(data);
        };

        var _onStateChange = function (event) {
            switch (event.data) {
                case YT.PlayerState.PLAYING:
                    _videoTracking.onVideoPlayed();
                    _interval = setInterval(_onIntervalFired, 250);
                    break;
                case YT.PlayerState.ENDED:
                    _videoTracking.onVideoEnded();
                    clearInterval(_interval);
                    break;
                default:
                    break;
            }
        };

        var _reset = function () {
            _videoTracking.reset();
            _attachTrackingEvents();
        }

        var _attachTrackingEvents = function () {
            self.videoStarted().then(SC.main.vimeoVideo);
            self.video25PercentWatched().then(SC.main.vimeoVideo);
            self.video50PercentWatched().then(SC.main.vimeoVideo);
            self.video75PercentWatched().then(SC.main.vimeoVideo);
            self.videoCompleted().then(function (adobeTrackingEvent) {
                SC.main.vimeoVideo(adobeTrackingEvent);
                _reset();
            });
        }

        var _init = function () {
            _id = options.id;

            if ($(_id).lengt == 0)
                return;

            _videoTracking.clickSource = options.clickSource;
            _iframeContainer = options.iframeContainer;
            options.videoPlayerConfig = options.videoPlayerConfig || {};
            options.videoPlayerConfig.events = options.videoPlayerConfig.events || {};
            options.videoPlayerConfig.events['onReady'] = options.videoPlayerConfig.events['onReady'] || _onPlayerReady;
            options.videoPlayerConfig.events['onStateChange'] = options.videoPlayerConfig.events['onStateChange'] || _onStateChange;

            self.onPlayerReady = options.onPlayerReady;
            self.videoPlayerConfig = options.videoPlayerConfig
            self.videoPlayer = new YT.Player(_id, self.videoPlayerConfig);

            _attachTrackingEvents();
        }

        self.videoStarted = function () { return _videoTracking.videoStarted(); };

        self.videoCompleted = function () { return _videoTracking.videoCompleted(); };

        self.video25PercentWatched = function () { return _videoTracking.video25PercentWatched(); };

        self.video50PercentWatched = function () { return _videoTracking.video50PercentWatched(); };

        self.video75PercentWatched = function () { return _videoTracking.video75PercentWatched(); };

        _init();
    };

    $.LsYoutubeVideoCollection = {
        items: [],
        init: function () {
            var self = this;

            //--wait for api to load
            $(self).on('ls.youtubeapiready', function () {
                //--banner video
                if ($('#ctaWatchVideo').length > 0)
                    self.items.push(new $.LsYoutTubeVideo({
                        id: document.getElementById('bannerVideo'),
                        onPlayerReady: function (videoPlayer, isIOS) {
                            $('#ctaWatchVideo').on('click', function (event) {
                                event.stopPropagation();

                                if ($('#lightstreamVideo').css('display') === 'block') {
                                    $('#lightstreamVideo').hide();
                                    $('#ctaWatchVideo').text('Watch our video');
                                    videoPlayer.pauseVideo();
                                }
                                else {
                                    if (isIOS)
                                        videoPlayer.cueVideoById(videoPlayer.getVideoData()['video_id']);
                                    else
                                        videoPlayer.playVideo();

                                    $('#ctaDescriptions').hide();
                                    $('#lightstreamVideo').delay(250).show(0);
                                    $('#ctaWatchVideo').text('Hide the video');
                                }
                            });
                        }
                    }));

                //--all other yt videos
                $('iframe[src*="www.youtube.com/embed"]:not(#bannerVideo)').each(function (index, iframe) {
                    self.items.push(new $.LsYoutTubeVideo({
                        id: document.getElementById($(iframe).attr('id')),
                    }));
                });
            });

            //load api
            var tag = document.createElement('script');
            tag.src = "https://www.youtube.com/iframe_api";
            var firstScriptTag = document.getElementsByTagName('script')[0];
            firstScriptTag.parentNode.insertBefore(tag, firstScriptTag);
        }
    }
}(jQuery));

$.LsYoutubeVideoCollection.init();

function onYouTubeIframeAPIReady() {
    $('document').ready(function () {
        $($.LsYoutubeVideoCollection).trigger('ls.youtubeapiready');
    });
};
