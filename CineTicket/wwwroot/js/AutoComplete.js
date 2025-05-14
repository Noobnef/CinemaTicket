var jq = jQuery.noConflict();
jq(document).ready(function () {
    jq("#SearchProduct").autocomplete({
        source: function (request, response) {
            jq.ajax({
                url: "/Movie/Search",
                type: "GET",
                dataType: "json",
                data: { term: request.term },
                success: function (data) {
                    response(jq.map(data, function (item) {
                        return {
                            label: item.label,
                            value: item.label,
                            id: item.value
                        };
                    }));
                },
                error: function () {
                    console.log("Error loading autocomplete data.");
                }
            });
        },
        minLength: 2,
        select: function (event, ui) {
            jq("#SearchProduct").val(ui.item.label);
            window.location.href = "/Product/Display/" + ui.item.id;
            return false;
        }
    });
});