'use strict';

//for "group_by" the comparison data
var DataGrouper = (function() {
  var has = function(obj, target) {
    return _.any(obj, function(value) {
      return _.isEqual(value, target);
    });
  };

  var keys = function(data, names) {
    return _.reduce(data, function(memo, item) {
      var key = _.pick(item, names);
      if (!has(memo, key)) {
        memo.push(key);
      }
      return memo;
    }, []);
  };

  var group = function(data, names) {
    var stems = keys(data, names);
    return _.map(stems, function(stem) {
      return {
        key: stem,
        vals:_.map(_.where(data, stem), function(item) {
          return _.omit(item, names);
        })
      };
    });
  };

  group.register = function(name, converter) {
    return group[name] = function(data, names) {
      return _.map(group(data, names), converter);
    };
  };

  return group;
}());

DataGrouper.register("group_types", function(item) {
  return _.extend({}, item.key, {buy_sell: _.reduce(item.vals, function(memo, node) {
    return memo + Number(node.buy_sell);
  }, 0),
      construction: _.reduce(item.vals, function(memo, node) {
        return memo + Number(node.construction);
      }, 0),
      rent: _.reduce(item.vals, function(memo, node) {
        return memo + Number(node.rent);
      }, 0)

    }
  );
});

var formatter;
function updateFormatter(dateRange) {
  formatter = d3.time.format((dateRange > 12) ? '%m/%y':'%d/%m/%y');
}

angular.module('kenseeApp')
  .controller('DashboardCtrl', function ($scope, $http, SERVER_URL) {
    $scope.dateRange = 144;
    updateFormatter($scope.dateRange);
    $scope.country = "United States";

    // note: there is no real need in complicating the data series with country, but if we do it now, it will be easier
    // when multiple countries will be needed.
    // (it is especially annoying when it needs to be a property of an object like in colors_object or axes_object)
    // se la vi.
    $scope.country_sentiment = "Sentiment - " + $scope.country;
//    $scope.country_building_starts = "Building Starts - " + $scope.country;
    $scope.country_house_price_index = "House Price Index - " + $scope.country;
    $scope.parseHighlights = function()
    {
      $scope.CBREHighlights = $scope.highlights.filter(function (el) {
        return el.topic == "CBRE";
      });
      $scope.colliersHighlights = $scope.highlights.filter(function (el) {
        return el.topic == "Colliers";
      });
      $scope.cushmanHighlights = $scope.highlights.filter(function (el) {
        return el.topic == "Cushman & Wakefield";
      });
    };

    $scope.currentComparisonSelection = "Retail";

    $scope.preparePropertycomparisonData = function() {
      $scope.comparison_buy_sell_relative = ['buy/sell'];
      $scope.comparison_construction_relative = ['construction'];
      $scope.comaprison_dates = ['x'];
      $scope.comparison_data.forEach(function (point) {
        if (point.property == $scope.currentComparisonSelection) {
          var total = point.buy_sell + point.construction;
          $scope.comparison_buy_sell_relative.push(point.buy_sell / total);
          $scope.comparison_construction_relative.push(point.construction / total);
          $scope.comaprison_dates.push(point.date);
        }
      });
    };

    $scope.refreshPropertycomparisonGraph = function() {
      $scope.preparePropertycomparisonData();
      $scope.comparisonChart.load({
        columns: [
          $scope.comaprison_dates,
          $scope.comparison_buy_sell_relative,
          $scope.comparison_construction_relative
        ]
      })
    };

    $scope.preparePropertycomparisonGraph = function() {
      $scope.preparePropertycomparisonData();
      var types_obj = {};
      types_obj['buy/sell'] = 'area';
      types_obj['construction'] = 'area';
      var colors_obj = {};
      colors_obj['buy/sell'] = '#069546';
      colors_obj['construction'] = '#FFCC00';

      $scope.comparisonChart = c3.generate({
        bindto: '#comparisonChart',
        padding: {top: 90, right: 25, bottom: 20, left: 50},
        legend: {
          position: 'inset',
          inset: {
            anchor: 'top-right', x: 20, y: -78, step: 4},
          item: {onclick: function (id) {
            //do nothing - default is removing the series
          }
          }
        },
        data: {
          x: 'x',
          xFormat: '%d-%m-%Y',
          columns: [
            $scope.comaprison_dates,
            $scope.comparison_buy_sell_relative,
            $scope.comparison_construction_relative
          ],
          colors: colors_obj,
          types: types_obj,
          groups: [['construction', 'buy/sell']],
          order: null
        },
        point: { show: false },
        axis: {
          x: {
            type: 'timeseries',
            tick: {format: function (x) {
              return formatter(x);
            }, rotate: 60 }
          },
          y: {
            tick: {
              format: d3.format('%')
            },
            min: 0, max: 1,
            padding: {top: 0, bottom: 0}
          }
        }
      });
    }

    $scope.prepareTotalcomparisonData = function() {
      $scope.comparison_buy_sell = ['buy/sell'];
      $scope.comparison_construction = ['construction'];
      $scope.comaprison_dates = ['x'];
      var grouped_data = DataGrouper.group_types($scope.comparison_data, ["date"]);
      grouped_data.forEach(function (point) {
        $scope.comparison_buy_sell.push(point.buy_sell);
        $scope.comparison_construction.push(point.construction);
        $scope.comaprison_dates.push(point.date);
      });

    };

    $scope.refreshTotalcomparisonGraph = function() {
      $scope.prepareTotalcomparisonData();
      $scope.comparisonChartBars.load({
        columns: [
          $scope.comaprison_dates,
          $scope.comparison_buy_sell,
          $scope.comparison_construction
        ]
      })
    };

    $scope.prepareTotalcomparisonGraph = function() {
      $scope.prepareTotalcomparisonData();
      var types_obj = {};
      types_obj['buy/sell'] = 'bar';
      types_obj['construction'] = 'bar';
      var colors_obj = {};
      colors_obj['buy/sell'] = '#FF6965';
      colors_obj['construction'] = '#1AABFF';

      $scope.comparisonChartBars = c3.generate({
        bindto: '#comparisonChartBars',
        padding: {top: 90, right: 25, bottom: 20, left: 50},
        legend: {
          position: 'inset',
          inset: {anchor: 'top-right', x: 20, y: -78, step: 4},
          item: {onclick: function (id) {
              //do nothing - default is removing the series
            }
          }
        },
        data: {
          x: 'x',
          xFormat: '%d-%m-%Y',
          columns: [
            $scope.comaprison_dates,
            $scope.comparison_buy_sell,
            $scope.comparison_construction
          ],
          colors: colors_obj,
          types: types_obj
        },
        axis: {
          x: {
            type: 'timeseries',
            tick: {format: function (x) {
              return formatter(x);
            }, rotate: 60 }
          },
          y: {
            tick: {
              format: d3.format('d')
            }
          }
        }
      });
    };

    $scope.prepareConstructionSpendingAreaData = function() {
      $scope.lodging = ['Lodging'];
      $scope.office = ['Office'];
      $scope.commercial = ['Commercial'];
      $scope.healthCare = ['HealthCare'];
      $scope.leisure = ['Leisure'];
      $scope.macro_dates = ['x'];
      $scope.macro.forEach(function (point) {
        $scope.lodging.push(point.RateForLodging);
        $scope.office.push(point.RateForOffice);
        $scope.commercial.push(point.RateForCommercial);
        $scope.healthCare.push(point.RateForHealthcare);
        $scope.leisure.push(point.RateForLeasure);
        $scope.macro_dates.push(point.date);
      });
    };

    $scope.refreshConstructionSpendingAreaGraph = function() {
      $scope.prepareConstructionSpendingAreaData();
      $scope.constructionAreaChart.load({
        columns: [
          $scope.macro_dates,
          $scope.lodging,
          $scope.office,
          $scope.commercial,
          $scope.healthCare,
          $scope.leisure
        ]
      })
    };

    $scope.prepareConstructionSpendingAreaGraph = function() {
      $scope.prepareConstructionSpendingAreaData();

      var types_obj = {};
      types_obj['Lodging'] = 'area';
      types_obj['Office'] = 'area';
      types_obj['Commercial'] = 'area';
      types_obj['HealthCare'] = 'area';
      types_obj['Leisure'] = 'area';

      $scope.constructionAreaChart = c3.generate({
        bindto: '#constructionAreaGraph',
        padding: {top: 90, right: 25, bottom: 20, left: 50},
        legend: {
          position: 'inset',
          inset: {
            anchor: 'top-right', x: 20, y: -88, step: 5
          },
          item: {
            onclick: function (id) {
              //do nothing - default is removing the series
            },
          }
        },
        data: {
          x: 'x',
          xFormat: '%d-%m-%Y',
          columns: [
            $scope.macro_dates,
            $scope.lodging,
            $scope.office,
            $scope.commercial,
            $scope.healthCare,
            $scope.leisure
          ],
//          colors: colors_obj,
          types: types_obj,
          groups: [['Lodging', 'Office', 'Commercial', 'HealthCare', 'Leisure']],
          order: null
        },
        axis: {
          x: {
            type: 'timeseries',
            tick: {format: function (x) {
              return formatter(x);
            }, rotate: 60 }
          },
          y: {
            tick: {
              format: d3.format(',')
            }
          }
        }
      });

    };

    $scope.prepareConstructionSpendingLineData = function() {
      $scope.non_residential = ['Non Residential'];
      $scope.residential = ['Residential'];
      $scope.macro_dates = ['x'];
      $scope.macro.forEach(function (point) {
        $scope.non_residential.push(point.RateForNonResidential);
        $scope.residential.push(point.RateForResidential);
        $scope.macro_dates.push(point.date);
      });
    };

    $scope.refreshConstructionSpendingLineGraph = function() {
      $scope.prepareConstructionSpendingLineData();
      $scope.constructionLineChart.load({
        columns: [
          $scope.non_residential,
          $scope.residential,
          $scope.macro_dates
        ]
      })

    };

    $scope.prepareConstructionSpendingLineGraph = function() {
      $scope.prepareConstructionSpendingLineData();

      var types_obj = {};
      types_obj['Non Residential'] = 'line';
      types_obj['Residential'] = 'line';

      $scope.constructionLineChart = c3.generate({
        bindto: '#constructionLineGraph',
        padding: {top: 90, right: 25, bottom: 20, left: 50},
        legend: {
          position: 'inset',
          inset: {
            anchor: 'top-right', x: 20, y: -78, step: 4
          },
          item: {
            onclick: function (id) {
              //do nothing - default is removing the series
            },
          }
        },
        data: {
          x: 'x',
          xFormat: '%d-%m-%Y',
          columns: [
            $scope.macro_dates,
            $scope.non_residential,
            $scope.residential
          ],
//          colors: colors_obj,
          types: types_obj,
        },
        axis: {
          x: {
            type: 'timeseries',
            tick: {format: function (x) {
              return formatter(x);
            }, rotate: 60 }
          },
          y: {
            tick: {
              format: d3.format(',')
            }
          }
        }
      });

    };

    $scope.prepareMacroData = function() {
      $scope.UnemploymentRate = ['Unemployment Rate'];
      $scope.Inflation = ['Inflation'];
//      $scope.HousePriceIndexChange = ['House Price Index Change'];
      $scope.macro_dates = ['x'];
      $scope.macro.forEach(function (point) {
        $scope.UnemploymentRate.push(point.UnemploymentRate);
        $scope.Inflation.push(point.Inflation);
//          $scope.HousePriceIndexChange.push(point.HousePriceIndexChange);
        $scope.macro_dates.push(point.date);
      });
    };

    $scope.refreshMacroGraph = function() {
      $scope.prepareMacroData();
      $scope.macroChart.load({
        columns: [
          $scope.macro_dates,
          $scope.UnemploymentRate,
          $scope.Inflation
        ]
      })
    };

    $scope.prepareMacroGraph = function() {
      $scope.prepareMacroData();

      var types_obj = {};
      types_obj['Unemployment Rate'] = 'area';
      types_obj['Inflation'] = 'area';
//      types_obj['House Price Index Change'] = 'area';

      var colors_obj = {};
      colors_obj['Unemployment Rate'] = '#4D92C0';
      colors_obj['Inflation'] = '#FCE2CB';
//      colors_obj['House Price Index Change'] = '#48BB8B'

      $scope.macroChart = c3.generate({
        bindto: '#macroChart',
        padding: {top: 90, right: 25, bottom: 20, left: 50},
        legend: {
          position: 'inset',
          inset: {
            anchor: 'top-right', x: 20, y: -78, step: 4
          },
          item: {
            onclick: function (id) {
              //do nothing - default is removing the series
            },
          }
        },
        data: {
          x: 'x',
          xFormat: '%d-%m-%Y',
          columns: [
            $scope.macro_dates,
            $scope.UnemploymentRate,
            $scope.Inflation
//            $scope.HousePriceIndexChange
          ],
//          colors: colors_obj,
          types: types_obj
        },
        axis: {
          x: {
            type: 'timeseries',
            tick: {format: function (x) {
              return formatter(x);
            }, rotate: 60 }
          }
        }
      });
    };

    $scope.prepareSentimentData = function()
    {
      $scope.market_sentiment_value = [$scope.country_sentiment];
      //$scope.building_starts_value = [$scope.country_building_starts];
      $scope.house_price_value = [$scope.country_house_price_index];
      $scope.market_sentiment_dates = ['x'];

      $scope.market_sentiment.forEach(function(point) {
        $scope.market_sentiment_value.push(point.sentiment);
//        $scope.building_starts_value.push(point.newResidentialConstruction);
        $scope.house_price_value.push(point.nationalHomePriceIndex);
        $scope.market_sentiment_dates.push(point.date);
      });
    };

    $scope.refreshSentimentGraph = function()
    {
      $scope.prepareSentimentData();
      $scope.sentimentChart.load({
        columns: [
          $scope.market_sentiment_dates,
          $scope.market_sentiment_value,
          //$scope.building_starts_value
          $scope.house_price_value
        ]
      })
    };

    $scope.prepareSentimentGraph = function()
    {
      $scope.prepareSentimentData();
      var axes_object = {};
      axes_object[$scope.country_sentiment] = 'y';
      //axes_object[$scope.country_building_starts] = 'y2';
      axes_object[$scope.country_house_price_index] = 'y2';
      var colors_object = {};
      colors_object[$scope.country_sentiment] = "#48BB8B";
      //colors_object[$scope.country_building_starts] = "#FF7F0E";
      colors_object[$scope.country_house_price_index] = "#FF7F0E";
      $scope.sentimentChart = c3.generate({
        bindto: '#sentimentChart',
        padding: {
          top: 90,
          right: 65,
          bottom: 20,
          left: 50
        },
        legend: {
          position: 'inset',
          inset: {
            anchor: 'top-right',
            x: 20,
            y: -78,
            step: 4
          },
          item:{
            onclick: function (id) {
              //do nothing - default is removing the series
            }
          }
        },
        data: {
          x: 'x',
          xFormat: '%d-%m-%Y',
          columns: [
            $scope.market_sentiment_dates,
            $scope.market_sentiment_value,
//            $scope.building_starts_value
            $scope.house_price_value
          ],
          axes: axes_object,
          colors: colors_object
        },
        axis: {
          x: {
            type: 'timeseries',
            tick: {format: function (x) {
              return formatter(x);
            }}
          },
          y: {
            label: {
              text: 'Sentiment',
              position: 'outer-middle'
            },
            tick: {
              format: d3.format('.1f')
            }
          },
          y2: {
            show: true,
            label: {
//              text: 'Building starts',
              text: 'House Price Index',
              position: 'outer-middle'
            },
            tick: {
              format: d3.format('.1f')
            }
          }
        }
      });
    }

    $scope.changeDateRange = function(months)
    {
      $scope.dateRange = months;
      updateFormatter($scope.dateRange);
      $scope.highlights = $scope["highlights_" + months];
      $scope.market_sentiment = $scope["market_sentiment_" + months];
      $scope.comparison_data = $scope["comparison_data_" + months];
      $scope.general_info = $scope["general_info_" + months];
      $scope.macro = $scope["macro_" + months];

      $scope.refreshSentimentGraph();
//      $scope.refreshPropertycomparisonGraph();
////      $scope.refreshTotalcomparisonGraph();
      $scope.refreshConstructionSpendingAreaGraph();
      $scope.refreshConstructionSpendingLineGraph();
      $scope.refreshMacroGraph();
      $scope.parseHighlights();
    };

    $scope.refreshAllData = function(country) {
      var default_period = '144';
      var request = SERVER_URL + "dashboard_data/" + default_period + "/" + country;
      $scope.gridBusy = $http.get(request)
        .success(function (data) {
          $scope.highlights = $scope["highlights_" + default_period] = data.highlights;
          $scope.market_sentiment = $scope["market_sentiment_" + default_period] = data.market_sentiment;
          $scope.comparison_data = $scope["comparison_data_" + default_period] = data.comparison_data;
          $scope.macro = $scope["macro_" + default_period] = data.macro;
          $scope.general_info = $scope["general_info_" + default_period] = data.generalInformation;

          $scope.prepareSentimentGraph();
          //$scope.preparePropertycomparisonGraph();
          //$scope.prepareTotalcomparisonGraph();
          $scope.prepareConstructionSpendingAreaGraph();
          $scope.prepareConstructionSpendingLineGraph();
          $scope.prepareMacroGraph();
          $scope.parseHighlights();
        });

      var extra_time_periods = ['3', '6', '12', '36'];
      extra_time_periods.forEach(function(period) {
        var request = SERVER_URL + "dashboard_data/" + period + "/" + country;
        $scope[period + "_valid"] = false;
        $http.get(request)
          .success(function (data) {
            $scope["highlights_" + period] = data.highlights;
            $scope["market_sentiment_" + period] = data.market_sentiment;
            $scope["comparison_data_" + period] = data.comparison_data;
            $scope["macro_" + period] = data.macro;
            $scope["general_info_" + period] = data.generalInformation;
            $scope[period + "_valid"] = true;
          });
      });

    };

    $scope.refreshAllData($scope.country.toLocaleLowerCase());

  });
