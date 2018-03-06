function ServiceChartsHelper(serviceId) {

    this.serviceId = serviceId;

    this.drawChart = function (agent, result) {
        var data = new google.visualization.DataTable();
        data.addColumn('string', 'name');
        data.addColumn('number', 'value');

        var dataArray = [];

        $.each(result, function (i, obj) {
            var myVal = parseFloat(result[i].value);
            dataArray.push([obj.name, myVal]);
        });

        data.addRows(dataArray);

        var barchart_options = {
            title: 'Сумарна кількість замовлень',
            width: 200,
            height: 200
        };

        var piechart_options = {
            title: 'Відсоткове відношення замовлень',
            width: 200,
            height: 200
        };

        var barchart = new google.visualization.BarChart(
            document.getElementById(agent + '_barchart_div')
            );

        var piechart = new google.visualization.PieChart(
            document.getElementById(agent + '_piechart_div')
            );

        barchart.draw(data, barchart_options);
        piechart.draw(data, piechart_options);
    };

    this.isWrongInput = function () {
        return isNaN(parseFloat($('input[name="year"]').val())) ||
                    parseFloat($('input[name="year"]').val()) <= 1900 ||
                    isNaN(parseFloat($('select[name="month"]').val()));
    };

    this.getMonthApplications = function () {
        var toPass = {
            year: $('input[name="year"]').val(),
            month: $('select[name="month"]').val(),
            service_id: this.serviceId
        };

        var self = this;

        $.ajax({
            method: "POST",
            url: "/Service/GetMonthApplications",
            contentType: "application/json",
            data: JSON.stringify(toPass),
            success: function (res) {
                if (res != null) {

                    $("#service_container").append(res);

                    var result = [
                        {
                            name: 'No Bill Data',
                            value: $('input[name="service_no_bill_items"]').val()
                        },
                        {
                            name: 'Advance Pending Data',
                            value: $('input[name="service_advance_pending_items"]').val()
                        },
                        {
                            name: 'Main Pending Data',
                            value: $('input[name="service_main_pending_items"]').val()
                        },
                        {
                            name: 'Main Paid Data',
                            value: $('input[name="service_main_paid_items"]').val()
                        }
                    ];

                    google.charts.load('current', {
                        'packages': ['corechart']
                    });

                    google.charts.setOnLoadCallback(function () {
                        self.drawChart('service', result);
                    });
                }
                else {
                    $("#service_container").append('<p>Інформації не знайдено</p>');
                }
            },
            error: function (res) {
                $("#service_container").append('<p>Інформації не знайдено</p>');
            }
        });
    };

    this.clearState = function () {
        $("#service_container").attr('hidden', false);

        $("#service_container").empty();

        $("#feedback_toggle").attr('hidden', true);

        $("#feedback_container").attr('hidden', true);

        $("#consultant_container").attr('hidden', true);
    };

    this.getConsultantApplications = function () {

        if ($('select[name="consultant_email"]').val() == null) {
            return;
        }

        $("#consultant_model").val($('select[name="consultant_email"]').val());

        $("#feedback_container").attr('hidden', true);

        $("#consultant_container").empty();
        $("#consultant_container").removeAttr('hidden');

        var toPass = {
            year: $('input[name="year"]').val(),
            month: $('select[name="month"]').val(),
            service_id: this.serviceId,
            email: $('select[name="consultant_email"]').val()
        };

        var self = this;

        $.ajax({
            method: "POST",
            url: "/Service/GetConsultantApplications",
            contentType: "application/json",
            data: JSON.stringify(toPass),
            success: function (res) {
                if (res != null) {

                    $("#consultant_container").append(res);

                    var result = [
                 {
                     name: 'No Bill Data',
                     value: $('input[name="consultant_no_bill_items"]').val()
                 },
                 {
                     name: 'Advance Pending Data',
                     value: $('input[name="consultant_advance_pending_items"]').val()
                 },
                 {
                     name: 'Main Pending Data',
                     value: $('input[name="consultant_main_pending_items"]').val()
                 },
                 {
                     name: 'Main Paid Data',
                     value: $('input[name="consultant_main_paid_items"]').val()
                 }
                    ];


                    google.charts.load('current', {
                        'packages': ['corechart']
                    });

                    google.charts.setOnLoadCallback(function () {
                        self.drawChart('consultant', result);
                    });

                    $("#feedback_toggle").removeAttr('hidden');
                }
                else {
                    $("#consultant_container").prepend('<p>Інформації не знайдено</p>');
                }
            },
            error: function (res) {
                $("#consultant_container").prepend('<p>Інформації не знайдено</p>');
            }
        });
    };
}