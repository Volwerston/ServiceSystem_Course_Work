/// <reference path="jquery-1.10.2.js" />

var BillValidator = function () {
    this.Validate = function () {
        var toReturn = true;

        if ($('select[name="price_measure"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#price_measure_validation").removeAttr("hidden");
            $("#price_measure_validation").text("Введіть день початку");
        } else {
            $(this).parent().removeClass("has-error");
            $("#price_measure_validation").attr('hidden', true);
        }

        if ($('input[name="advance_percent"]').val() == "" ||
    isNaN(parseFloat($('input[name="advance_percent"]').val())) ||
    parseFloat($('input[name="advance_percent"]').val()) <= 0 ||
       parseFloat($('input[name="advance_percent"]').val()) >= 100) {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#advance_percent_validation").removeAttr("hidden");
            $("#advance_percent_validation").text("Введіть відсоток передоплати");
        } else {
            $(this).parent().removeClass("has-error");
            $("#advance_percent_validation").attr('hidden', true);
        }

        if ($('input[name="price"]').val() == "" ||
    isNaN(parseFloat($('input[name="price"]').val())) ||
    parseFloat($('input[name="price"]').val()) <= 0) {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#price_validation").removeAttr("hidden");
            $("#price_validation").text("Введіть кількість одиниць");
        } else {
            $(this).parent().removeClass("has-error");
            $("#price_validation").attr('hidden', true);
        }

        if ($('input[name="date_limit_advance"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#date_limit_advance_validation").removeAttr("hidden");
            $("#date_limit_advance_validation").text("Введіть крайній день передоплати");
        } else {
            $(this).parent().removeClass("has-error");
            $("#date_limit_advance_validation").attr('hidden', true);
        }



        if ($('input[name="date_limit_main"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#date_limit_main_validation").removeAttr("hidden");
            $("#date_limit_main_validation").text("Введіть крайній день основного платежу");
        } else {
            $(this).parent().removeClass("has-error");
            $("#date_limit_main_validation").attr('hidden', true);
        }

        if ($('input[name="has_time_limit"]').is(":checked")) {

            if ($('input[name="time_limit_advance"]').val() == "") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#time_limit_advance_validation").removeAttr("hidden");
                $("#time_limit_advance_validation").text("Введіть крайній час передоплати");
            } else {
                $(this).parent().removeClass("has-error");
                $("#time_limit_advance_validation").attr('hidden', true);
            }


            if ($('input[name="time_limit_main"]').val() == "") {
                toReturn = false;
                $(this).parent().addClass("has-error");
                $("#time_limit_main_validation").removeAttr("hidden");
                $("#time_limit_main_validation").text("Введіть крайній час основного платежу");
            } else {
                $(this).parent().removeClass("has-error");
                $("#time_limit_main_validation").attr('hidden', true);
            }
        }

        return toReturn;
    }
}

var WebmoneyBillValidator = function () {
    this.Validate = function () {
        var toReturn = true;
        var prot = WebmoneyBillValidator.prototype.Validate();

        if ($('input[name="wm_purse"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#wm_purse_validation").removeAttr("hidden");
            $("#wm_purse_validation").text("Введіть Ваш WMU гаманець");
        }
        else {
            $(this).parent().removeClass("has-error");
            $("#wm_purse_validation").attr('hidden', true);
        }

        return toReturn && prot;
    }
}

WebmoneyBillValidator.prototype = new BillValidator();

var BankBillValidator = function () {
    this.Validate = function () {
        var toReturn = true;
        var prot = BankBillValidator.prototype.Validate();

        if ($('input[name="account"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#account_validation").removeAttr("hidden");
            $("#account_validation").text("Введіть номер Вашого рахунку");
        }
        else {
            $(this).parent().removeClass("has-error");
            $("#account_validation").attr('hidden', true);
        }

        if ($('input[name="edrpou"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#edrpou_validation").removeAttr("hidden");
            $("#edrpou_validation").text("Введіть ЄДРПОУ Вашого банку");
        }
        else {
            $(this).parent().removeClass("has-error");
            $("#edrpou_validation").attr('hidden', true);
        }

        if ($('input[name="mfo"]').val() == "") {
            toReturn = false;
            $(this).parent().addClass("has-error");
            $("#mfo_validation").removeAttr("hidden");
            $("#mfo_validation").text("Введіть МФО Вашого банку");
        }
        else {
            $(this).parent().removeClass("has-error");
            $("#mfo_validation").attr('hidden', true);
        }


        return toReturn && prot;
    }
}

BankBillValidator.prototype = new BillValidator();