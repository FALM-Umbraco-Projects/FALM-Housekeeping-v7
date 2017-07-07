'use strict';
app.requires.push('angularUtils.directives.dirPagination');
(function () {
    // Create DB Edit Controller for DB Events
    function LogsDBManagerController($route, $scope, $routeParams, $filter, hkLogsResource, userService, notificationsService, localizationService, dialogService, navigationService, eventsService) {
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

        // GET - VIEW LOGS
        $scope.showLoader = false;
        $scope.showSearchPanel = false;
        $scope.showNoDBLogsFound = false;

        // Get all logs via hkLogsResource
        hkLogsResource.getDBLogs().then(function (response) {
            $scope.showSearchPanel = true;
            $scope.showLoader = true;
            $scope.logs = response.data;
            $scope.sortType = 'LogDate'; // set the default sort type
            $scope.reverse = true;       // set the default sort order

            if ($scope.logs.ListDBLogs.length == "0") {
                $scope.showNoDBLogsFound = true;
            }
            $scope.showLoader = false;   // hide loader
        });

        // Table search
        $scope.q = '';

        // Table sort
        $scope.sortType = '';
        $scope.reverse = false;
        $scope.sort = function (keyname) {
            $scope.sortKey = keyname;           //set the sortKey to the param passed
            $scope.reverse = !$scope.reverse;   //if true make it false and vice versa
        }

        // Table pagination
        $scope.currentPage = 1;
        $scope.pageSize = 10;
        $scope.totalPages = 1;

        // Label CSS Class
        $scope.getLabelType = function (LogHeader) {
            switch (LogHeader) {
                case 'Error':
                case 'LoginFailure':
                    return 'label label-danger';
                    break;
                case 'Publish':
                    return 'label label-success';
                    break;
                case 'UnPublish':
                    return 'label label-info';
                    break;
                case 'SendToPublish':
                case 'SendToTranslate':
                    return 'label label-warning';
                    break;
                default:
                    return 'label label-default';
            };
            return 'label label-default';
        };

        // Open detail modal
        $scope.openDetailsModal = function (logItem, filteredLogs) {
            var dialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/logs-dbmanager-modal-details.html',
                dialogData: {
                    logItem: logItem,
                    filteredLogs: filteredLogs
                },
                show: true,
                width: 800
            });
        }

        // POST - DELETE VIEWED LOGS
        $scope.logsSuccessNotification = {
            'type': 'success',
            'sticky': false
        };
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsSuccessHeadline").then(function (value) {
            $scope.logsSuccessNotification.headline = value;
        });
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsSuccessMessage").then(function (value) {
            $scope.logsSuccessNotification.message = value;
        });

        $scope.logsErrorNotification = {
            "type": "error",
            "headline": "",
            "sticky": false
        };
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsErrorHeadline").then(function (value) {
            $scope.logsErrorNotification.headline = value;
        });
        localizationService.localize("FALM_LogsManager.Cleanup.DeleteLogsErrorMessage").then(function (value) {
            $scope.logsErrorNotification.message = value;
        });

        localizationService.localize("FALM_LogsManager.Cleanup.ConfirmationMessage").then(function (value) {
            $scope.confirmDeleteActionMessage = value;
        });

        $scope.showDeletePanel = false;
        $scope.showDeleteLoader = false;

        // Delete filtered Logs via hkLogsResource
        $scope.deleteFilteredDBLogs = function (filteredLogs) {
            if (confirm($scope.confirmDeleteActionMessage)) {
                $scope.showSearchPanel = false;
                $scope.showDeletePanel = true;
                $scope.showDeleteLoader = true;
                hkLogsResource.deleteFilteredDBLogs(filteredLogs).then(function (response) {
                    if (response.data = true) {
                        notificationsService.add($scope.logsSuccessNotification);
                        $route.reload();
                    }
                    else {
                        notificationsService.add($scope.logsErrorNotification);
                        $scope.showDeletePanel = false;
                    }
                });
            }
        };
    };

    // Create Edit controller for Trace Logs
    function LogsTLManagerController($route, $scope, $routeParams, $filter, hkLogsResource, userService, notificationsService, localizationService, dialogService, navigationService, eventsService) {
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

        // Table search
        $scope.q = '';

        // Table sort
        $scope.sortType = '';
        $scope.reverse = false;
        $scope.sort = function (keyname) {
            $scope.sortKey = keyname;           //set the sortKey to the param passed
            $scope.reverse = !$scope.reverse;   //if true make it false and vice versa
        }

        // Table pagination
        $scope.currentPage = 1;
        $scope.pageSize = 10;
        $scope.totalPages = 1;

        // GET - VIEW TRACE LOGS
        $scope.showLoader = true;
        $scope.showSearchPanel = true;
        $scope.showNoTLLogsFound = false;

        // Get all logs via hkLogsResource
        hkLogsResource.getTraceLogs($scope.id).then(function (response) {
            $scope.showSearchPanel = true;
            $scope.showLoader = true;
            $scope.logs = response.data;
            $scope.sortType = 'LogDate'; // set the default sort type
            $scope.reverse = true;       // set the default sort order

            if ($scope.logs.ListTraceLogs.length == "0") {
                $scope.showNoDBLogsFound = true;
            }

            $scope.showLoader = false;   // hide loader
        });

        // Label CSS Class
        $scope.getLabelType = function (LogLabel) {
            switch (LogLabel) {
                case 'ERROR':
                    return 'label label-danger';
                    break;
                case 'INFO':
                    return 'label label-info';
                    break;
                case 'WARN':
                    return 'label label-warning';
                    break;
                default:
                    return 'label label-default';
            };
            return 'label label-default';
        };

        // Open detail modal
        $scope.openDetailsModal = function (logItem, filteredLogs) {
            var dialog = dialogService.open({
                template: '/App_Plugins/FALM/backoffice/housekeeping/view/logs-tlmanager-modal-details.html',
                dialogData: {
                    logItem: logItem,
                    filteredLogs: filteredLogs
                },
                show: true,
                width: 800
            });
        }
    };

    // Register controllers
    angular.module("umbraco").controller("FALMHousekeepingLogsDBManagerController", LogsDBManagerController);
    angular.module("umbraco").controller("FALMHousekeepingLogsTLManagerController", LogsTLManagerController);
})();