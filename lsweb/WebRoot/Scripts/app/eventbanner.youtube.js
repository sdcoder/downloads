(function ($) {
    $($.LsYoutubeVideoCollection).on('ls.youtubeapiready', function () {
        var vidId = "RFswH6XY4dE";        
        var cmsYoutubeId = $('#cms-youtube-id');
        if (cmsYoutubeId.length > 0) {
            vidId = cmsYoutubeId.text();
        }
        $.LsYoutubeVideoCollection.items.push(new $.LsYoutTubeVideo({
            id: document.getElementById('lightstreamVideo'),
            videoPlayerConfig: {
                height: '100%',
                width: '100%',
                videoId: vidId,
                playerVars: {
                    'controls': 1,
                    'rel': 0,
                    'showinfo': 0,
                    'modestbranding': 0,
                    'autoplay': 0,
                    'enablejsapi': 1,
                    'fs': 0
                }
            },
            onPlayerReady: function (videoPlayer, isIOS) {
                $('#headlinePlayButtonDesktop').on('click', function (event) {
                    event.stopPropagation();
                    if ($('#lightstreamVideo').css('display') === 'block') {

                        $('#lightstreamVideo').hide();
                        videoPlayer.pauseVideo();
                    }
                    else {
                        if (isIOS) {
                            videoPlayer.cueVideoById(videoPlayer.getVideoData()['video_id']);
                        }
                        else {
                            videoPlayer.playVideo();
                        }
                        $('#lightstreamVideo').delay(250).show(0);
                    }
                });
            }
        }));
        $.LsYoutubeVideoCollection.items.push(new $.LsYoutTubeVideo({
            id: document.getElementById('lightstreamVideoMobile'),
            videoPlayerConfig: {
                height: '100%',
                width: '100%',
                videoId: vidId,
                playerVars: {
                    'controls': 1,
                    'rel': 0,
                    'showinfo': 0,
                    'modestbranding': 0,
                    'autoplay': 0,
                    'enablejsapi': 1,
                    'fs': 0
                }
            }
        }));
    });
}(jQuery));
