'use strict';
(function () {
    // Create Edit controller
    function LogsCleanupController($scope, $routeParams, hkMediaResource, userService, notificationsService, localizationService, eventsService) {
        // Set a property on the scope equal to the current route id
        $scope.id = $routeParams.id;

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
    };

    // Register the controller
    angular.module("umbraco").controller("FALMHousekeepingLogsCleanupController", LogsCleanupController);
})();