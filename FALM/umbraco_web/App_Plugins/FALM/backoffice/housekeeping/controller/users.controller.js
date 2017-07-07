'use strict';
(function () {
    // Create Edit controller
    function UsersCleanupController($route, $scope, $routeParams, hkUsersResource, userService, notificationsService, localizationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

        // Reload page function
        $scope.reloadRoute = function () {
            $route.reload();
        }

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

        $scope.userSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_UsersManager.Cleanup.DeleteSelectedUsersSuccessHeadline").then(function (value) {
            $scope.userSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_UsersManager.Cleanup.DeleteSelectedUsersSuccessMessage").then(function (value) {
            $scope.userSuccessNotification.message = value;
        });

        $scope.userErrorNotification = {
            "type": "error",
            "headline": "",
            "sticky": false
        };
        localizationService.localize("FALM_UsersManager.Cleanup.DeleteSelectedUsersErrorHeadline").then(function (value) {
            $scope.userErrorNotification.headline = value;
        });
        localizationService.localize("FALM_UsersManager.Cleanup.DeleteSelectedUsersErrorMessage").then(function (value) {
            $scope.userErrorNotification.message = value;
        });
        
        localizationService.localize("FALM_UsersManager.Cleanup.ConfirmDeleteActionMessage").then(function (value) {
            $scope.confirmDeleteActionMessage = value;
        });
        localizationService.localize("FALM_UsersManager.Cleanup.NoUserSelectedMessage").then(function (value) {
            $scope.noUserSelectedMessage = value;
        });

        // Check if at least one user is selected
        $scope.isAtLeastOneUserSelected = function () {
            for (var u in $scope.users) {
                var user = $scope.users[u];
                if (user.Selected)
                    return true;
            }
            return false;
        };

        // Select and Unselect all Users
        $scope.checkAll = function (obj) {
            angular.forEach($scope.users, function (user) {
                user.Selected = obj.selectAll;
            });
        };

        // Get all users via hkUsersResource
        hkUsersResource.getAllUsers().then(function (response) {
            $scope.users = response.data;
        });

        // Post users list to delete via hkUsersResource
        $scope.deleteSelectedUsers = function (selectedUsers) {
            if (confirm($scope.confirmDeleteActionMessage)) {
                hkUsersResource.deleteSelectedUsers(selectedUsers).then(function (response) {
                    if (response.data = true) {
                        notificationsService.add($scope.userSuccessNotification);
                        $route.reload();
                    }
                    else {
                        notificationsService.add($scope.userErrorNotification);
                    }
                });
            }
        };
    };

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingUsersCleanupController", UsersCleanupController);
})();