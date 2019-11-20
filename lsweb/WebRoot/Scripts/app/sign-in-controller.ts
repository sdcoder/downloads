angular.module('LightStreamApp')
    .controller('SignInController', function ($scope, webStorage) {
        var clearSession = function () {
            webStorage.session.clear();

            if (window.history && window.history.replaceState) {
                window.history.replaceState({}, 'Last Page', '/customer-sign-in');
            }
        };

        $scope.init = function (data) {
            var signInModel = $.parseJSON(data)
            $scope.UserId = signInModel.UserId;

            $scope.IsTempUserId = signInModel.IsTempUserId;
            $scope.UserIdBackground = signInModel.IsTempUserId ? '#ddd' : 'white';

            if (signInModel.IsSignOut) {
                clearSession();
            }
        };
    });

// omniture
SC.main.signIn();

$('form').on('submit', function () {
    return true;
});