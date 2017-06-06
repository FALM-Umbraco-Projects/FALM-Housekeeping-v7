'use strict';

// Create DB Edit Controller for DB Events (Details modal)
function LogsDBManagerDetailsController($scope) {
    $scope.logTypeLabel = '';

    switch ($scope.dialogData.logItem.LogHeader) {
        case 'Error':
        case 'LoginFailure':
            $scope.logTypeLabel = 'danger';
            break;
        case 'Publish':
            $scope.logTypeLabel = 'success';
            break;
        case 'UnPublish':
            $scope.logTypeLabel = 'info';
            break;
        case 'SendToPublish':
        case 'SendToTranslate':
            $scope.logTypeLabel = 'warning';
            break;
        default:
            $scope.logTypeLabel = 'default';
    };

    $scope.closeDialog = function () {
        $scope.dismiss();
    };
}

// Create DB Edit Controller for Trace Logs (Details modal)
function LogsTLManagerDetailsController($scope) {
    $scope.logTypeLabel = '';

    switch ($scope.dialogData.logItem.LogLevel) {
        case 'ERROR':
            $scope.logTypeLabel = 'danger';
            break;
        case 'INFO':
            $scope.logTypeLabel = 'info';
            break;
        case 'WARN':
            $scope.logTypeLabel = 'warning';
            break;
        default:
            $scope.logTypeLabel = 'default';
    };

    $scope.closeDialog = function () {
        $scope.dismiss();
    };
}

// Register controllers
angular.module("umbraco").controller("FALMHousekeepingLogsDBManagerDetailsController", LogsDBManagerDetailsController);
angular.module("umbraco").controller("FALMHousekeepingLogsTLManagerDetailsController", LogsTLManagerDetailsController);