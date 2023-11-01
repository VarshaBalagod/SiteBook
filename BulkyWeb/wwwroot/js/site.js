// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
//$(document).ready(function () {

 //var url = window.location.toString();
//var u = url.substring(0, url.lastIndexOf("/")).toString();
//alert('url - ' + url + 'u - ' + u);
//alert(location.pathname.split("/")[1]);
//url - https://localhost:7148/
//u - https://localhost:7148

//url - https://localhost:7148/Customer/Home/Privacy
//u - https://localhost:7148/Customer/Home

//url - https://localhost:7148/Admin/Category
//u - https://localhost:7148/Admin

//var parser = document.createElement('a');
//parser.href = "http://example.com:3000/pathname/?search=test#hash";

//parser.protocol; // => "http:"
//parser.hostname; // => "example.com"
//parser.port;     // => "3000"
//parser.pathname; // => "/pathname/"
//parser.search;   // => "?search=test"
//parser.hash;     // => "#hash"
//parser.host;     // => "example.com:3000"

$(document).ready(function () {
    //if (location.pathname != "/") {
    //    if (location.pathname.split("/")[1] == 'Admin') {
    //        //alert(3);
    //        $('.dropdown-toggle').addClass('active');            
    //    }
    //    else {
    //       // alert(1);
    //        $('nav a[href^="/' + location.pathname.split("/")[1] + '"]').addClass('active');
    //    }
    //}
    //else {
    //    //alert(2);
    //    $('.home-link').addClass('active');
    //}
    $('.nomultiple').removeAttr('multiple');
});
