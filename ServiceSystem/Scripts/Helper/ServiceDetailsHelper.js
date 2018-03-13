function ServiceDetailsHelper(serviceId, userEmail) {

    this.confirmConsultant = null;
    this.id = null;
    this.faq_id = null;
    this.validator = null;
    this.media_id = null;
    this.serviceId = serviceId;
    this.userEmail = userEmail;

    this.toggleService = function (isActive) {

        var self = this;

        $.ajax({
            method: "POST",
            url: "/api/ServiceApi/ChangeServiceStatus",
            headers: {
                Authorization: 'Bearer ' + localStorage.getItem("access_token"),
            },
            contentType: "application/json; charset=utf-8",
            data: JSON.stringify({ IsActive: isActive, Id: self.serviceId }),
            success: function (res) {
                window.location = '/Service/ServiceDetails?id=' + self.serviceId;
            },
            error: function (res) {
                displayMessage("Error", JSON.stringify(res));
            }
        });
    }

    this.getDivForFAQ = function(faq) {
        return `<div class="row faqItem" id="faq_${faq.Id}">
                <a href="#" class ="close pull-right" id="close_${faq.Id}">
                <span class ="glyphicon glyphicon-remove"></span></a>
                <div class="col-xs-12">
                <h4 class="text-center">Запитання</h4>
                </div>
                <div class="col-xs-12 faqItem__header">
                <p>${faq.Question}</p>
                </div>
                <div class="col-xs-12">
                <h4 class="text-center">Відповідь</h4>
                </div>
                <div class="col-xs-12 faqItem__header">
                <p>${faq.Answer}</p>
                </div>
                </div>`;
    }

    this.setValidator = function () {
        if ($('input[name="applicationType"]').val() == "Session") {
            this.validator = new SessionValidator();
        }
        else if ($('input[name="applicationType"]').val() == "Deadline") {
            this.validator = new DeadlineAppValidator();
        }
    }

    this.inputInvalid = function () {
        return this.validator != null && !this.validator.Validate();
    }

    this.deleteFaq = function () {

        var self = this;
        $("#faq_delete_modal").modal('hide');

        $.ajax({
            method: "DELETE",
            url: "/api/FAQ/Delete?id=" + self.faq_id,
            contentType: "application/json; charset=utf-8",
            success: function (res) {
                if ($("#faq_container").children().length == 1) {
                    $("#faq_container").empty();
                    $("#faq_container").append('<p class="empty_message text-center">Запитань не знайдено.</p>');
                }
                else {
                    $("#faq_" + self.faq_id).hide();
                }
            },
            error: function (res) {
                displayMessage("Error", res.responseText);
            }
        });
    }

    this.deleteMedia = function () {
        $("#media_delete_modal").modal('hide');
        var self = this;

        $.ajax({
            method: "DELETE",
            url: "/api/ServiceApi/DeleteMediaFile?id=" + self.media_id,
            headers: {
                Authorization: 'Bearer ' + localStorage.getItem("access_token"),
            },
            contentType: "application/json; charset=utf-8",
            success: function (res) {
                if ($("#media_container").children().length == 1) {
                    $("#media_container").empty();
                }
                else {
                    $("#media_" + self.media_id).hide();
                }
            },
            error: function (res) {
                displayMessage("Error", res.responseText);
            }
        });

    }

    this.addFaq = function () {

        var self = this;

        if ($('input[name="question"]').val() == "") {
            displayMessage("Warning", "Заповніть поле запитання");
            return;
        }

        if ($('textarea[name="answer"]').val() == "") {
            displayMessage("Warning", "Заповніть поле відповіді");
            return;
        }

        var toPass = {
            Id: 0,
            ServiceId: this.serviceId,
            Question: $('input[name="question"]').val(),
            Answer: $('textarea[name="answer"]').val()
        };

        $.ajax({
            method: "POST",
            url: "/api/FAQ/Post",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(toPass),
            success: function (res) {
                if ($("#faq_container").find(".empty_message").length > 0) {
                    $("#faq_container").empty();
                }

                toPass.Id = JSON.parse(res);

                $("#faq_container").append(self.getDivForFAQ(toPass));

                $('#close_' + toPass.Id).click(function (e) {
                    e.preventDefault();

                    self.faq_id = toPass.Id;

                    $("#faq_delete_modal").modal('show');
                });
            },
            error: function (res) {
                displayMessage("Warning", res.responseText);
            }
        });

        this.createDialogue = function () {
            var toPass = {
                ServiceId: this.serviceId,
                CustomerEmail: this.userEmail
            };

            var self = this;

            $.ajax({
                method: "POST",
                url: "/api/Dialogue/CreateDialogue",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("access_token"),
                },
                data: JSON.stringify(toPass),
                success: function (res) {
                    displayMessage("Success", "Діалог успішно створено. Для перегляду діалогу перейдіть в Особистий кабінет -> Діалоги");
                },
                error: function (res) {
                    displayMessage("Warning", res.responseText);
                }
            });
        }

        this.deleteConsultant = function () {
            var self = this;

            $.ajax({
                method: "POST",
                url: "/api/ServiceConsultants/DeleteConsultant",
                contentType: "application/json; charset=utf-8",
                dataType: "json",
                headers: {
                    Authorization: 'Bearer ' + localStorage.getItem("access_token"),
                },
                data: JSON.stringify(self.id),
                success: function (res) {
                    $("#consultant_" + self.id).remove();

                    if ($("#consultants_list").children().length == 0) {
                        $("#panel_body_1").empty();
                        $("#panel_body_1").append('<p class="text-center">Жодного консультанта не знайдено</p>');
                    }
                },
                error: function (res) {
                    displayMessage("Warning", res.responseText);
                }
            });
        }
    }

    this.consultantConfirm = function () {

        var self = this;

        $.ajax({
            method: "POST",
            url: "/api/ServiceConsultants/ConfirmConsultant",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            headers: {
                Authorization: 'Bearer ' + localStorage.getItem("access_token"),
            },
            data: JSON.stringify(self.confirmConsultant),
            success: function (res) {
                $("#consultant_request").remove();
                $("#ask_modal").modal('hide');
            },
            error: function (res) {
                displayMessage("Warning", res.responseText);

                $("#ask_modal").modal('hide');
            }
        });
    }

    this.htmlDecode = function () {
        var paramDivs = $(".paramText");
        var self = this;

        paramDivs.each(function (index, element) {

            var innerElements = $(this).children();

            innerElements.each(function (i, elem) {

                var data = $(this).html();

                data = data.replace(/&gt;/g, ">");
                data = data.replace(/&lt;/g, "<");
                data = data.replace(/&quot;/, "\"");

                $(this).empty();

                $(this).html(data);


                $("#application_create").click(function () {

                    $("#application_holder").removeAttr("hidden");

                    $("body").animate({ scrollTop: $(document).height() }, 1000);

                    if ($('input[name="applicationType"]').val() == "Deadline") {
                        self.validator = new FsdDeadlineAppValidator();
                        $("#from_some_date").removeAttr("hidden");
                        $("#by_last_date").attr("hidden", true);
                    }

                });

                $('input[name="deadline_type"]').change(function () {

                    if ($(this).val() == "from_some_date") {
                        self.validator = new FsdDeadlineAppValidator();
                        $("#from_some_date").removeAttr("hidden");
                        $("#by_last_date").attr("hidden", true);
                    }
                    else if ($(this).val() == "by_last_date") {
                        self.validator = new BldDeadlineAppValidator();
                        $("#by_last_date").removeAttr("hidden");
                        $("#from_some_date").attr("hidden", true);
                    }
                });
            });
        });
    }
}