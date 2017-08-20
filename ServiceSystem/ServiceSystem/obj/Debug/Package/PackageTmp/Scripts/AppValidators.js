/// <reference path="jquery-1.10.2.js" />

var SessionValidator = function () {
     
    this.Validate = function () {
        var toReturn = true;

        if ($('input[name="start_time"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#start_time_validation").removeAttr("hidden");
            $("#start_time_validation").text("Введіть час початку сеансу");
        } else {
            $(this).parent().removeClass("has-error");
            $("#start_time_validation").attr('hidden', true);
        }

        if ($('input[name="service_day"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#service_day_validation").removeAttr("hidden");
            $("#service_day_validation").text("Введіть день сеансу");
        } else {
            $(this).parent().removeClass("has-error");
            $("#service_day_validation").attr('hidden', true);
        }

        return toReturn;
    }
}

var DeadlineAppValidator = function () {

    this.Validate = function () {
        var toReturn = true;

        if ($('input[name="deadline_type"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#deadline_type_validation").removeAttr("hidden");
            $("#deadline_type_validation").text("Введіть час завершення");
        }
        else {
            $(this).parent().removeClass("has-error");
            $("#deadline_type_validation").attr('hidden', true);
        }

        return toReturn;
    }

}

var BldDeadlineAppValidator = function () {

    this.Validate = function () {
        var prot = BldDeadlineAppValidator.prototype.Validate();

        var toReturn = true;

        if ($('input[name="deadline_last_time"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#deadline_last_time_validation").removeAttr("hidden");
            $("#deadline_last_time_validation").text("Введіть час завершення");
        } else {
            $(this).parent().removeClass("has-error");
            $("#deadline_last_time_validation").attr('hidden', true);
        }

        if ($('input[name="deadline_last_date"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#deadline_last_date_validation").removeAttr("hidden");
            $("#deadline_last_date_validation").text("Введіть день завершення");
        } else {
            $(this).parent().removeClass("has-error");
            $("#deadline_last_date_validation").attr('hidden', true);
        }

        return toReturn && prot;
    }
}

BldDeadlineAppValidator.prototype = new DeadlineAppValidator();

var FsdDeadlineAppValidator = function () {

    this.Validate = function () {
        var prot = FsdDeadlineAppValidator.prototype.Validate();

        var toReturn = true;

        if ($('input[name="deadline_start_time"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#deadline_start_time_validation").removeAttr("hidden");
            $("#deadline_start_time_validation").text("Введіть час початку");
        } else {
            $(this).parent().removeClass("has-error");
            $("#deadline_start_time_validation").attr('hidden', true);
        }

        if ($('input[name="deadline_start_date"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#deadline_start_date_validation").removeAttr("hidden");
            $("#deadline_start_date_validation").text("Введіть день початку");
        } else {
            $(this).parent().removeClass("has-error");
            $("#deadline_start_date_validation").attr('hidden', true);
        }

        if ($('input[name="deadline_duration"]').val() == "") {
            toReturn = false;
            $("#deadline_duration_validation").removeAttr("hidden");
            $("#deadline_duration_validation").text("Введіть тривалість");
        } else {
            $("#deadline_duration_validation").attr('hidden', true);
        }

        return toReturn && prot;
    }
}

FsdDeadlineAppValidator.prototype = new DeadlineAppValidator();