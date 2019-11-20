/*globals angular, console, $*/
// VerificationUploadController 
(function () {
    'use strict';
    angular.module('ApplicationStatusModule')
        .controller('VerificationUploadController', ['$scope', '$upload',
        function ($scope, $upload) {
            // private functions
            var uploadFile = function (file, $parentDiv, verificationType, applicantType, applicationId, verificationRequestId, itemId) {
                $scope.upload = $upload.upload({
                    url: '/appstatus/upload',
                    method: 'POST',
                    //headers: {'header-key': 'header-value'},
                    //withCredentials: true,
                    data: {
                        myObj: $scope.myModelObj,
                        applicationId: applicationId,
                        verificationRequestId: verificationRequestId
                    },
                    file: file // or list of files ($files) for html5 only
                    //fileName: 'doc.jpg' or ['1.jpg', '2.jpg', ...] // to modify the name of the file(s)
                    // customize file formData name ('Content-Desposition'), server side file variable name. 
                    //fileFormDataName: myFile, //or a list of names for multiple files (html5). Default is 'file' 
                    // customize how data is added to formData. See #40#issuecomment-28612000 for sample code
                    //formDataAppender: function(formData, key, val){}
                }).xhr(function () {
                    if ($parentDiv) {
                        $parentDiv.find('[data-status=success]').hide();
                        $parentDiv.find('.upload-progress-bar-container').show();
                    }
                }).progress(function (evt) {
                    if ($parentDiv) {
                        $parentDiv.find('[data-status=success]').hide();
                        $parentDiv.find('.upload-progress-bar-container').show();
                        var progress = 80.0 * evt.loaded / evt.total;
                        $parentDiv.find('.upload-progress-bar').css('width', progress + '%');
                        if (progress === 80) {
                            $parentDiv.find('.upload-progress-bar').animate({
                                width: "100%"
                            }, file.size / 10240); // assume about a second for a 10MB file
                        }
                    }
                }).error(function () {
                    addError('There was an error with your upload. Please try again.', undefined);
                }).success(function (data) {
                    if ($parentDiv) {
                        $parentDiv.find('.upload-progress-bar-container').hide();
                        addNotReadyDoc(itemId);
                        if (data.Success) {
                            $parentDiv.attr('data-success', true);
                            $parentDiv.find('[data-status=success]').show();
                            var $filenames = $parentDiv.find('[data-field=filename]');
                            if ($filenames) {
                                if (!$filenames.data('names')) {
                                    $filenames.data('names', []);
                                }
                                $filenames.data('names').push(data.FileName);
                            }
                            if ($filenames.data('names').length > 1) {
                                $parentDiv.find('[data-field=filename]').text($filenames.data('names').length + ' files');
                            }
                            else {
                                $parentDiv.find('[data-field=filename]').text($filenames.data('names').join(', '));
                            }
                            $parentDiv.find('.status-icon img').attr('src', '/Content/images/icon-success.png');
                        }
                        else {
                            $parentDiv.attr('data-success', false);
                            addError(data.ErrorMessage, itemId);
                        }
                    }
                    if ($('.boxed[data-success=true]').length === $('.boxed').length) {
                        $scope.AllDocumentsUploaded = true;
                    }
                });
            }, addError = function (msg, id) {
                $scope.ErrorMessages.push({
                    id: id,
                    errorMessage: msg
                });
                clearNotReadyDoc(id);
            }, clearErrors = function (id) {
                $scope.ErrorMessages = $scope.ErrorMessages.filter(function (error) {
                    return error.id != id;
                });
            }, addNotReadyDoc = function (id) {
                if ($scope.notReadyDocuments.indexOf(id) === -1) {
                    $scope.notReadyDocuments.push(id);
                }
            }, clearNotReadyDoc = function (idToClear) {
                $scope.notReadyDocuments = $scope.notReadyDocuments.filter(function (id) {
                    return id != idToClear;
                });
            }, isAllowedFileType = function (fileName) {
                var isAllowed = false;
                if (fileName.indexOf('.') !== -1) {
                    var allowedFileTypes = ['jpg', 'jpeg', 'gif', 'png', 'tif', 'tiff', 'pdf'];
                    var splitFileName = fileName.split('.');
                    var extension = splitFileName[splitFileName.length - 1];
                    var isAllowed = allowedFileTypes.indexOf(extension.toLowerCase()) !== -1;
                }
                return isAllowed;
            };
            $scope.ErrorMessages = [];
            $scope.notReadyDocuments = [];
            $scope.hasRelevantErrors = function (itemId) {
                var filteredErrors = $scope.ErrorMessages.filter(function (error) {
                    return error.id == itemId;
                });
                return $scope.ErrorMessages && filteredErrors.length > 0;
            };
            $scope.isDocumentReady = function (itemId) {
                var filteredNotReadyDocs = $scope.notReadyDocuments.filter(function (id) {
                    return id == itemId;
                });
                return !($scope.notReadyDocuments && filteredNotReadyDocs.length > 0);
            };
            // "public" scope-level functions
            $scope.dragOverClass = function ($event) {
                var items = $event.dataTransfer.items, hasFile = false, i;
                if (items !== null && items !== undefined) {
                    for (i = 0; i < items.length; i += 1) {
                        if (items[i].kind === 'file') {
                            hasFile = true;
                            break;
                        }
                    }
                }
                else {
                    hasFile = true;
                }
                return hasFile ? "boxed-hover" : "boxed-error";
            };
            $scope.directoryDropped = function (id) {
                clearErrors(id);
                addError('Directory upload is not supported', id);
            };
            $scope.onFileSelect = function ($files, id, verificationType, applicantType, applicationId, verificationRequestId) {
                clearErrors(id);
                var $parentDiv = $('#' + id), file, i;
                //$files: an array of files selected, each file has name, size, and type.
                for (i = 0; i < $files.length; i += 1) {
                    file = $files[i];
                    if (file && file.size && file.size > 10240000) {
                        addError('File size exceeded. Please select a smaller document and resubmit.', id);
                    }
                    else if (!isAllowedFileType(file.name)) {
                        addError('Please review the format for document ' + file.name + ' and re-submit.', id);
                    }
                    else {
                        uploadFile(file, $parentDiv, verificationType, applicantType, applicationId, verificationRequestId, id);
                    }
                }
            };
        }]);
}());
//# sourceMappingURL=VerificationUploadController.js.map