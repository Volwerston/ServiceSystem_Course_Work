function ConsultantSearchHelper(serviceId) {

    this.lastId = 0;
    this.tempName = "";
    this.tempOrganisation = "";
    this.serviceId = serviceId;

    this.addMailConsultant = function () {
        var toPass = {
            Email: $('input[name="invitation_email"]').val(),
            ServiceId: this.serviceId
        }

        $.ajax({
            method: "POST",
            url: "/api/ServiceConsultants/AddMailConsultant",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify(toPass),
            success: function (res) {
                alert(res);
            },
            error: function (res) {
                displayMessage("Error", res.responseText);
            }
        });
    };

    this.makeNewSearch = function () {
        $("#servicesData").empty();

        this.lastId = 0;
        this.tempName = $('input[name="name"]').val();
        this.tempOrganisation = $('input[name="organisation"]').val();

        var toPass = {
            Name: this.tempName,
            Organisation: this.tempOrganisation,
            LastID: this.lastId,
            ServiceId: this.serviceId
        };

        var self = this;

        $.ajax({
            type: "POST",
            url: "/api/ServiceConsultants/PostConsultantParams",
            dataType: "json",
            headers: {
                Authorization: 'Bearer ' + localStorage.getItem("access_token")
            },
            data: JSON.stringify(toPass),
            contentType: "application/json; charset=utf-8",
            success: function (res) {

                var toProceed = JSON.parse(res);

                if (toProceed != null) {
                    for (var i = 0; i < toProceed.length; ++i) {
                        $("#servicesData").append(self.getDivForConsultant(toProceed[i])).children().last().fadeIn("slow");;
                    }

                    this.lastId += toProceed.length;
                }
                else {
                    if ($("#servicesData").children().length == 0) {
                        $("#servicesData").append('<div class="text-center">Користувачів не знайдено</div>');
                    }
                }
            },
            error: function (res) {
                displayMessage("Error", res.responseText);
            }
        });
    };

    this.loadNextChunk = function () {
        var toPass = {
            Name: this.tempName,
            Organisation: this.tempOrganisation,
            LastID: this.lastId,
            ServiceId: this.serviceId
        };

        var self = this;

        $.ajax({
            type: "POST",
            url: "/api/ServiceConsultants/PostConsultantParams",
            dataType: "json",
            async: false,
            headers: {
                Authorization: 'Bearer ' + localStorage.getItem("access_token")
            },
            data: JSON.stringify(toPass),
            contentType: "application/json; charset=utf-8",
            async: false,
            success: function (res) {

                var toProceed = JSON.parse(res);

                if (toProceed != null) {

                    for (var i = 0; i < toProceed.length; ++i) {
                        $("#servicesData").append(self.getDivForConsultant(toProceed[i])).children().last().fadeIn("slow");
                    }

                    this.lastId += toProceed.length;
                }
                else {

                    if ($("#servicesData").children().length == 0) {
                        $("#servicesData").append('<div class="text-center">Користувачів не знайдено</div>');
                    }
                }
            },
            error: function (res) {
                displayMessage("Error", res.responseText);
            }
        });
    };

    this.inviteConsultant = function (id) {
        $("#loadingModal").modal("show");

        $.ajax({
            method: "POST",
            url: "/api/ServiceConsultants/AddConsultant",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            data: JSON.stringify({Id: id, ServiceId: this.serviceId}),
            success: function(res){
                $("#consultant_" + id).remove();
                $("#loadingModal").modal("hide");
                if($("#servicesData").children().length == 0){
                    $("#servicesData").append('<p style="text-align: center">Користувачів не знайдено</p>');
                }
                displayMessage("Success", 'Користувача успішно запрошено');
            },
            error: function(res){
                $("#loadingModal").modal("hide");
                displayMessage("Error", res.responseText);
            }
        });
    };

    this.getDivForConsultant = function (consultant) {
        return `<div class="row text-center dataUnit" id="consultant_${consultant.Id}">
               <div class="col-xs-10 col-md-offset-1 dataUnit__body">
               <h3>${consultant.Surname} ${consultant.Name} ${consultant.FatherName}</h3>
               <p class="text-center"><b>E-mail:</b>${consultant.Email}</p>
               <p class="text-center"><b>Організація:</b>${consultant.Organisation}</p>
               <div class="row">
               <div class="col-sm-6 col-md-4 col-sm-offset-3 col-md-offset-4">
               <input type="button" class="btn btn-success dataUnit__inviteBtn btn-block" name="invite_${consultant.Id}" value="Запросити">
               </div>
               </div>
               </div>
               </div>`;
    };


}