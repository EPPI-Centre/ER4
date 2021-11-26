// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
function postwith(to, p) {
    //https://mentaljetsam.wordpress.com/2008/06/02/using-javascript-to-post-data-between-pages/
    var myForm = document.createElement("form");
    myForm.style.display = "none";
    myForm.method = "post";
    myForm.action = to;
    for (var k in p) {
        var myInput = document.createElement("input");
        myInput.setAttribute("name", k);
        myInput.setAttribute("value", p[k]);
        myForm.appendChild(myInput);
    }
    document.body.appendChild(myForm);
    myForm.submit();
    document.body.removeChild(myForm);
};

function HTMLEncodeText(unsafeText) {
    return $('<div>').text(unsafeText).html();
}

function Buildnode(attr) {
    var nodes = [];

    var res = new kendo.data.Node({
        text: attr.setName ? attr.setName : attr.attributeName
    });
    for (var ii = 0; ii < attr.attributes.attributesList.length; ii++) {
        res.append(Buildnode(attr.attributes.attributesList[ii]));
    }
    res.id = attr.attributeId ? attr.attributeId : attr.setId;
    res.isCodeset = false;
    res.setId = attr.setId;
    //res.items = nodes;
    //res.hasChildren = () => {
    //    return this.items.length > 0;
    //};
    return res;
}
