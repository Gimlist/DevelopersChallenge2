app.factory("accessService", ["$http", function ($http) {

    var _p = undefined;
    var _loadError = undefined;
    // Request List of Statements
    var _load = function () {
        try {
            return $http.get("/Statement/List")
                .then((response) => {
                    return response.data;
                })
                .catch((error) => {
                    return error;
                });
        } catch (e) {
            return e;
        }
    };

    return {
        Load: _load,
        LoadError: _loadError
    };

}]);


