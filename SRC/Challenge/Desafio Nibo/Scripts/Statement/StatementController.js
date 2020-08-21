var app = angular.module('Statement', []);

app.controller('StatementController', function ($scope, accessService) {

    //Get object data from Statament
    accessService.Load().then((r) => {
        try {
            $scope.StatementsList = r;
        } catch{
            return r;
        }
    });

    // Initial upload status
    $scope.statusUpload = 5;

    //List of names of files
    $scope.FileName = [];

    // Upload Files function
    $scope.SendFile = function (fileInput) {
        Array.from(fileInput.files).forEach(function (file) {
            $scope.FileName.push(file.name);
            var CacheKeyStatement = new Date().getTime();

            // File name and extension validation
            if (/[a-z-A-Z-0-9]+(.ofx)/g.test(file.name)) {
                var formData = new window.FormData();

                formData.append("statement", file);
                formData.append("cacheKey", CacheKeyStatement);

                var uploadFile = new XMLHttpRequest(); // [upload status] :: : {0 = Not Allowed, 2 = "Success Upload", 3 = "Send Failure", 5 = "Idle"}

                uploadFile.open('POST', '/Statement/UploadFile');

                uploadFile.addEventListener("load", function (ev) {
                    $scope.$apply(function () {
                        if (ev.currentTarget.status === 200) {
                            $scope.statusUpload = 2;
                            delete $scope.StatementsList;
                            accessService.Load().then((r) => {
                                try {
                                    $scope.StatementsList = r;
                                } catch{
                                    return r;
                                }
                            });

                        }
                        else {
                            $scope.statusUpload = 3;
                            uploadFile.abort();
                        }
                    });
                }, true);

                // Send a Request
                uploadFile.send(formData);
            }
        });

    };

});



