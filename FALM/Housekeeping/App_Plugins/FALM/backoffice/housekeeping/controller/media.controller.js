'use strict';
(function () {
    // Create Edit controller
    function MediaCleanupController($route, $scope, $routeParams, hkMediaResource, userService, notificationsService, localizationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

        $scope.reloadRoute = function () {
            $route.reload();
        }

        $scope.currentUserLanguage = "en-GB";

        userService.getCurrentUser().then(function (user) {
            $scope.currentUserLanguage = user.locale;
        });

        // Select current treenode
        eventsService.on('appState.treeState.changed', function (event, args) {
            if (args.key === 'selectedNode') {

                function buildPath(node, path) {
                    path.push(node.id);
                    if (node.id === '-1') return path.reverse();
                    var parent = node.parent();
                    if (parent === undefined) return path;
                    return buildPath(parent, path);
                }

                event.currentScope.nav.syncTree({
                    tree: $routeParams.tree,
                    path: buildPath(args.value, []),
                    forceReload: false
                });
            }
        });

        // GET - VIEW MEDIA ORPHANS
        $scope.showSearchPanel = true;
        $scope.showLoader = true;
        $scope.showNoMediaOrphans = false;
        $scope.showDeletePanel = false;

        localizationService.localize("FALM_MediaManager.Cleanup.NoMediaOrphansMessage").then(function (value) {
            $scope.NoMediaOrphansMessage = value;
        });
        localizationService.localize("FALM_MediaManager.Cleanup.ConfirmationMessage").then(function (value) {
            $scope.confirmDeleteActionMessage = value;
        });

        // Get all users via hkUsersResource
        hkMediaResource.getMediaToDelete($scope.currentUserLanguage).then(function (response) {
            $scope.showSearchPanel = true;
            $scope.showLoader = true;
            $scope.showDeletePanel = false;
            $scope.showNoMediaOrphans = false;
            $scope.media = response.data;
            $scope.mediaToDelete = 0;
            if (($scope.media.ListMediaToDelete == null) || ($scope.media.ListMediaToDelete.length == "0")) {
                $scope.showNoMediaOrphans = true;
            }
            $scope.showLoader = false;
        });

        // POST - DELETE MEDIA ORPHANS

        // Post media orphans to delete via hkUsersResource
        $scope.deleteMediaOrphans = function (mediaOrphansToDelete) {
            if (confirm($scope.confirmDeleteActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = true;
                $scope.showLoader = true;
                hkMediaResource.postDeleteMediaOrphans(mediaOrphansToDelete).then(function (response) {
                    $scope.media = response.data;
                });
                $scope.showLoader = false;
            }
        };
    };

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingMediaCleanupController", MediaCleanupController);
})();