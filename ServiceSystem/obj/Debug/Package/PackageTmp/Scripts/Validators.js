/// <reference path="jquery-1.10.2.js" />

var Validator = function () {

    this.Validate = function () {

        var main = ValidateMain();
        var price = ValidatePrice();
        var prop = ValidateProperties();

        return main && price && prop;
    }

    var ValidateMain = function () {

        // name validation

        var toReturn = true;

        if ($('input[name="service_name"]').val() == "") {
            toReturn = false;

            $('input[name="service_name"]').parent().addClass("has-error");
            $("#service_name_validation").removeAttr("hidden");
            $("#service_name_validation").text("Введіть назву послуги");
        }
        else {
            $('input[name="service_name"]').parent().removeClass("has-error");
            $("#service_name_validation").attr('hidden', true);
        }

        // category validation

        if ($('select[name="service_category"]').val() == "None") {
            toReturn = false;
            $('select[name="service_category"]').parent().addClass("has-error");
            $("#service_category_validation").removeAttr("hidden");
            $("#service_category_validation").text("Оберіть категорію послуги");
        }
        else {
            $('select[name="service_category"]').parent().removeClass("has-error");
            $("#service_category_validation").attr('hidden', true);
        }

        // type validation

        if ($('select[name="service_type"]').val() == "none") {
            toReturn = false;
            $('select[name="service_type"]').parent().addClass("has-error");
            $("#service_type_validation").removeAttr("hidden");
            $("#service_type_validation").text("Оберіть тип послуги");
        }
        else {
            $('select[name="service_type"]').parent().removeClass("has-error");
            $("#service_type_validation").attr('hidden', true);
        }

        // advance percent validation

        if($('input[name="advance_percent"]').val() == "" ||
           $('input[name="advance_percent"]').val() <= 0  ||
           $('input[name="advance_percent"]').val() >= 100)
        {
            toReturn = false;
            $('input[name="advance_percent"]').parent().addClass("has-error");
            $("#advance_percent_validation").removeAttr("hidden");
            $("#advance_percent_validation").text("Неправильні дані для авансу");
        }
        else {
            $('input[name="advance_percent"]').parent().removeClass("has-error");
            $("#advance_percent_validation").attr('hidden', true);
        }
        
        return toReturn;
    }

    var ValidatePrice = function () {
        
        // currency validation

        var toReturn = true;

        var currencies = $('select[name^="currency"]');

        currencies.each(function (index, element) {

            var name = $(this).attr('name');

            if ($(this).val() == "none") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#" + name + "_validation").removeAttr("hidden");
                $("#" + name + "_validation").text("Оберіть валюту");
            } else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);
            }
        });

        // measure validation

        var propertyNames = $('input[name^="measure"]');

        propertyNames.each(function (index, element) {
            var name = $(this).attr('name');

            if ($(this).val() == "") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#" + name + "_validation").removeAttr("hidden");
                $("#" + name + "_validation").text("Введіть одиницю виміру");
            } else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);
            }
        });

        // price validation

        var propertyNames = $('input[name^="price"]');

        propertyNames.each(function (index, element) {

            if ($(this).attr('type') == "text") {
                var name = $(this).attr('name');

                if ($(this).val() == "" ||
                    isNaN(parseFloat($(this).val())) ||
                    parseFloat($(this).val()) <= 0) {
                    toReturn = false;
                    $(this).parent().addClass("has-error");
                    $("#" + name + "_validation").removeAttr("hidden");
                    $("#" + name + "_validation").text("Введіть ціну за одиницю виміру");
                } else {
                    $(this).parent().removeClass("has-error");
                    $("#" + name + "_validation").attr('hidden', true);
                }
            }
            });

        return toReturn;
    }

    var ValidateProperties = function () {

        var toReturn = true;
        // validate properties' types
        var propertyTypes = $('select[name^="property_type"]');

        propertyTypes.each(function (index, element) {

            var name = $(this).attr('name');

            if ($(this).val() == "none") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#" + name + "_validation").removeAttr("hidden");
                $("#" + name + "_validation").text("Оберіть категорію для властивості");
            } else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);
            }
        });

        //validate properties' names

        var propertyNames = $('input[name^="property_name"]');

        propertyNames.each(function (index, element) {
            var name = $(this).attr('name');

            if ($(this).val() == "") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#" + name + "_validation").removeAttr("hidden");
                $("#" + name + "_validation").text("Оберіть назву властивості");
            } else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);
            }
        });

        // validate properties' values

        var propertyValues = $('input[name^="default_widget_value"]');

        var textAreas = $('textarea[name^="default_widget_value"]');

        textAreas.each(function (index, element) {
            propertyValues.push($(this));
        });

        propertyValues.each(function (index, element) {
            var name = $(this).attr('name');

            if ($(this).val() == "") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#" + name + "_validation").removeAttr("hidden");
                $("#" + name + "_validation").text("Заповніть поле властивості");
            } else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);
            }
        });

        return toReturn;
    }
}

var DeadlineValidator = function () {
    
    this.Validate = function () {
        
        var time = ValidateTiming();
        var prot = DeadlineValidator.prototype.Validate();

        return time && prot;
    }

    var ValidateTiming = function () {

        var toReturn = true;

        var ranges = $('select[name^="range_type"]');

        ranges.each(function (index, element) {

            var name = $(this).attr('name');

            if ($(this).val() == "none") {
                toReturn = false;
                $('#' + name + "_validation").parent().addClass("has-error");
                $('#' + name + "_validation").removeAttr("hidden");
                $('#' + name + "_validation").text("Оберіть тип діапазону");
            }
            else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);

                var id = name.split('_')[2];

                if ($(this).val() == "from") {
                    if($('input[name="service_min_duration_' + id + '"]').val() == "" ||
                       isNaN($('input[name="service_min_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_min_duration_' + id + '"]').val()) <= 0)
                    {
                        toReturn = false;
                        $('#service_min_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_min_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_min_duration_' + id + '_validation').text("Введіть мінімальну тривалість")
                    }
                    else {
                        $('#service_min_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_min_duration_' + id + '_validation').attr('hidden', true);
                    }
                }
                else if ($(this).val() == "till") {
                    if ($('input[name="service_max_duration_' + id + '"]').val() == "" ||
                        isNaN($('input[name="service_max_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_max_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_max_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_max_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_max_duration_' + id + '_validation').text("Введіть максимальну тривалість")
                    }
                    else {
                        $('#service_max_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_max_duration_' + id + '_validation').attr('hidden', true);
                    }
                }
                else if ($(this).val() == "from_till") {
                    if ($('input[name="service_min_duration_' + id + '"]').val() == "" ||
                        isNaN($('input[name="service_min_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_min_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_min_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_min_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_min_duration_' + id + '_validation').text("Введіть мінімальну тривалість")
                    }
                    else {
                        $('#service_min_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_min_duration_' + id + '_validation').attr('hidden', true);
                    }

                    if ($('input[name="service_max_duration_' + id + '"]').val() == "" ||
                        isNaN($('input[name="service_max_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_max_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_max_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_max_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_max_duration_' + id + '_validation').text("Введіть максимальну тривалість")
                    }
                    else {
                        $('#service_max_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_max_duration_' + id + '_validation').attr('hidden', true);
                    }
                }
            }
        });

        return toReturn;
        }
}

DeadlineValidator.prototype = new Validator();

var SessionValidator = function () {

    this.Validate = function () {

        var days = ValidateDays();
        var time = ValidateTiming();

        var prot = SessionValidator.prototype.Validate();

        return days && time && prot;
    }

    var ValidateDays = function () {
        var days = ["mon", "tue", "wed", "thu", "fri", "sat", "sun"];

        var counter = 0;

        var toReturn = true;

        for (var i = 0; i < days.length; ++i) {
            if ($('input[name="' + days[i] + '"]').is(":checked")) {

                ++counter;

                var textToAdd = "";

                if ($('input[name="' + days[i] + 'StartTime"]').val() == "") {
                    textToAdd += "Введіть час початку сеансу<br/>";
                }

                if ($('input[name="' + days[i] + 'EndTime"]').val() == "") {
                    textToAdd += "Введіть час завершення сеансу<br/>";
                }

                if (textToAdd != "") {
                    toReturn = false;
                    $("#" + days[i] + "_validation").parent().addClass("has-error");
                    $("#" + days[i] + "_validation").removeAttr("hidden");
                    $("#" + days[i] + "_validation").html(textToAdd);
                }
                else {
                    $("#" + days[i] + "_validation").parent().removeClass("has-error");
                    $("#" + days[i] + "_validation").attr('hidden', true);
                }
            }
        }

        if (counter == 0) {
            toReturn = false;
            $("#all_validation").parent().addClass("has-error");
            $("#all_validation").removeAttr("hidden");
            $("#all_validation").text("Оберіть хоча б один день для проведення сеансу");
        }
        else {
            $("#all_validation").parent().removeClass("has-error");
            $("#all_validation").attr('hidden', true);
        }

        return toReturn;
    }

    var ValidateTiming = function () {
        
        var toReturn = true;

        var ranges = $('select[name^="range_type"]');

        ranges.each(function (index, element) {

            var name = $(this).attr('name');

            if ($(this).val() == "none") {
                toReturn = false;
                $('#' + name + "_validation").parent().addClass("has-error");
                $('#' + name + "_validation").removeAttr("hidden");
                $('#' + name + "_validation").text("Оберіть тип діапазону");
            }
            else {
                $(this).parent().removeClass("has-error");
                $("#" + name + "_validation").attr('hidden', true);

                var id = name.split('_')[2];

                if ($(this).val() == "from") {
                    if ($('input[name="service_min_duration_' + id + '"]').val() == "" ||
                       isNaN($('input[name="service_min_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_min_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_min_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_min_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_min_duration_' + id + '_validation').text("Введіть мінімальну тривалість")
                    }
                    else {
                        $('#service_min_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_min_duration_' + id + '_validation').attr('hidden', true);
                    }
                }
                else if ($(this).val() == "till") {
                    if ($('input[name="service_max_duration_' + id + '"]').val() == "" ||
                        isNaN($('input[name="service_max_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_max_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_max_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_max_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_max_duration_' + id + '_validation').text("Введіть максимальну тривалість")
                    }
                    else {
                        $('#service_max_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_max_duration_' + id + '_validation').attr('hidden', true);
                    }
                }
                else if ($(this).val() == "from_till") {
                    if ($('input[name="service_min_duration_' + id + '"]').val() == "" ||
                        isNaN($('input[name="service_min_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_min_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_min_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_min_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_min_duration_' + id + '_validation').text("Введіть мінімальну тривалість")
                    }
                    else {
                        $('#service_min_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_min_duration_' + id + '_validation').attr('hidden', true);
                    }

                    if ($('input[name="service_max_duration_' + id + '"]').val() == "" ||
                        isNaN($('input[name="service_max_duration_' + id + '"]').val()) ||
                        parseFloat($('input[name="service_max_duration_' + id + '"]').val()) <= 0) {
                        toReturn = false;
                        $('#service_max_duration_' + id + '_validation').parent().addClass("has-error");
                        $('#service_max_duration_' + id + '_validation').removeAttr("hidden");
                        $('#service_max_duration_' + id + '_validation').text("Введіть максимальну тривалість")
                    }
                    else {
                        $('#service_max_duration_' + id + '_validation').parent().removeClass("has-error");
                        $('#service_max_duration_' + id + '_validation').attr('hidden', true);
                    }
                }
            }
        });

        return toReturn;
    }
}

SessionValidator.prototype = new Validator();

var CourseValidator = function () {

    this.Validate = function () {
        

        var time = ValidateTiming();

        var prot = CourseValidator.prototype.Validate();

        return time && prot;
    }

    var ValidateTiming = function () {
        var toReturn = true;

        if($('input[name="service_start_date"]').val() == "" ||
           $('input[name="service_end_date"]').val() == "")
        {
            toReturn = false;
            $("#service_date_validation").parent().addClass("has-error");
            $("#service_date_validation").removeAttr("hidden");
        }
        else {
            $("#service_date_validation").parent().removeClass("has-error");
            $("#service_date_validation").attr('hidden', true);
        }

        if ($('input[name="is_time_defined"]').is(":checked")) {
            if ($('input[name="service_start_time"]').val() == "" ||
                $('input[name="service_end_time"]').val() == "") {
                toReturn = false;
                $("#service_time_validation").parent().addClass("has-error");
                $("#service_time_validation").removeAttr("hidden");
                $("#service_time_validation").text("Введіть час завершення курсу");
            }
            else {
                $("#service_time_validation").parent().removeClass("has-error");
                $("#service_time_validation").attr('hidden', true);
            }
        }

        return toReturn;
    }
}

CourseValidator.prototype = new Validator();

var DefinedCourseValidator = function () {

    this.Validate = function () {
        
        var time = ValidateTiming();

        var prot = DefinedCourseValidator.prototype.Validate();

        return time && prot;
    }

    var ValidateTiming = function () {

        var toReturn = true;

        if ($('select[name="week_gradation_type"]').val() == "none") {
            toReturn = false;
            $("#week_gradation_type_validation").parent().addClass("has-error");
            $("#week_gradation_type_validation").removeAttr("hidden");
            $("#week_gradation_type_validation").text("Введіть формат тижня")
        }
        else {
            $("#week_gradation_type_validation").parent().removeClass("has-error");
            $("#week_gradation_type_validation").attr("hidden", true);
        }


        if ($('select[name="week_gradation_type"]').val() == "day_hour") {
            var days = ["mon", "tue", "wed", "thu", "fri", "sat", "sun"];

            var counter = 0;

            for (var i = 0; i < days.length; ++i) {
                if ($('input[name="' + days[i] + '"]').is(":checked")) {

                    ++counter;

                    var textToAdd = "";

                    if ($('input[name="' + days[i] + 'StartTime"]').val() == "") {
                        textToAdd += "Введіть час початку сеансу<br/>";
                    }

                    if ($('input[name="' + days[i] + 'EndTime"]').val() == "") {
                        textToAdd += "Введіть час завершення сеансу<br/>";
                    }

                    if (textToAdd != "") {
                        toReturn = false;
                        $("#" + days[i] + "_validation").parent().addClass("has-error");
                        $("#" + days[i] + "_validation").removeAttr("hidden");
                        $("#" + days[i] + "_validation").html(textToAdd);
                    }
                    else {
                        $("#" + days[i] + "_validation").parent().removeClass("has-error");
                        $("#" + days[i] + "_validation").attr('hidden', true);
                    }
                }
            }

            if (counter == 0) {
                toReturn = false;
                $("#all_validation").parent().addClass("has-error");
                $("#all_validation").removeAttr("hidden");
                $("#all_validation").text("Оберіть хоча б один день для проведення сеансу");
            }
            else {
                $("#all_validation").parent().removeClass("has-error");
                $("#all_validation").attr('hidden', true);
            }
        }


        if ($('select[name="week_gradation_type"]').val() == "day_duration"){
            if($('select[name="number_of_days"]').val() == "none")
            {
                toReturn = false;
                $("#number_of_days_validation").parent().addClass("has-error");
                $("#number_of_days_validation").removeAttr("hidden");
                $("#number_of_days_validation").text("Оберіть кількість днів");
            }
            else {
                $("#number_of_days_validation").parent().removeClass("has-error");
                $("#number_of_days_validation").attr("hidden", true);

                //

                var durations = $('input[name$="Duration"]');

                durations.each(function (index, element) {

                    if($(this).val() == "" ||
                       isNaN(parseFloat($(this).val())) ||
                        parseFloat($(this).val()) <= 0) {
                        toReturn = false;
                        $("#" + $(this).attr('name') + "_validation").parent().addClass("has-error");
                        $("#" + $(this).attr('name') + "_validation").removeAttr("hidden");
                        $("#" + $(this).attr('name') + "_validation").text("Введіть кількість хвилин");

                    }
                    else {
                        $("#" + $(this).attr('name') + "_validation").parent().removeClass("has-error");
                        $("#" + $(this).attr('name') + "_validation").attr("hidden", true);
                    }
                        
                });
            }
        }

        return toReturn;
        }
    }

DefinedCourseValidator.prototype = new CourseValidator();
