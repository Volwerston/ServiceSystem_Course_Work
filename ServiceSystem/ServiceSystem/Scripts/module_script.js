/// <reference path="jquery-1.10.2.js" />


var myApp = angular.module("FeedbackApp", [])
            .controller("FeedbackController", function ($scope, $http, $log) {

                $scope.feedbacks = [];
                $scope.filteredFeedbacks = [];
                $scope.error = "";
                $scope.month = 1;

                $scope.showFeedbacks = function () {

                    $("#feedback_container").removeAttr('hidden');

                    $scope.email = $("#consultant_model").val();
                    $scope.serviceId = $("#service_id_data").val();

                    $scope.toPass = {
                        Email: $scope.email,
                        ServiceId: $scope.serviceId,
                        Month: $scope.month,
                        Year: $scope.year
                    }


                    $http.post('/api/ServiceConsultants/GetFeedbacks',$scope.toPass)
                         .then(function (response) {
                             $scope.feedbacks = response.data;
                         },
                         function (reason) {
                             $scope.error = reason.data;
                             $log.info($scope.error);
                         })
                }


                $scope.startIndex = 0;
                $scope.displayItems = 5;

                $scope.orderItem = "Date";
                $scope.isReverse = false;

                $scope.calculateOffset = function (type) {
                    $log.info("Filtered countries: " + $scope.filteredFeedbacks.length);

                    if (type == 'Next') {
                        if ($scope.startIndex + $scope.displayItems < $scope.filteredFeedbacks.length) {
                            $scope.startIndex += $scope.displayItems;
                        }
                    }
                    else {
                        if ($scope.startIndex - $scope.displayItems >= 0) {
                            $scope.startIndex -= $scope.displayItems;
                        }
                    }
                }

                $scope.getGlyph = function (item) {
                    var toReturn = "";

                    if ($scope.orderItem == item) {
                        toReturn = ($scope.isReverse ? "glyphicon-arrow-down" : "glyphicon-arrow-up");
                    }

                    return toReturn;
                }

                $scope.setOrder = function (item) {
                    $scope.isReverse = ($scope.orderItem == item) ? !$scope.isReverse : false;
                    $scope.orderItem = item;
                }
                
            });