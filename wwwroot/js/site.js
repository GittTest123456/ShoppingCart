window.onload = function () {
}

function incrementone(productId, price, username) {
    var count = Number(document.getElementById(productId).value);
    count += 1;
    document.getElementById(productId).value = count;
    var total = Number((document.getElementById("total").value).substr(1));
    total += price;
    document.getElementById("total").value = "$" + total.toString();
    AddQuantity(productId, username);
}

function decrementone(productId,price,username) {
    var count = Number(document.getElementById(productId).value);
    if (count == 1) {
        document.getElementById(productId).value = count;
    }
    else {
        count -= 1;
        document.getElementById(productId).value = count;
        var total = Number((document.getElementById("total").value).substr(1));
        total -= price;
        document.getElementById("total").value = "$" + total.toString();
        DecreaseQuantity(productId, username);
    }
}

function AddQuantity(productId, username) {
    let ajax = new XMLHttpRequest();

    ajax.open("POST", "/Home/AddQuantity");

    ajax.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
    

    ajax.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                return this.responseText;
            }
        }
    }
    ajax.send("ProductId=" + productId + "&username=" + username);
   
}

function DecreaseQuantity(productId, username) {
    let ajax = new XMLHttpRequest();

    ajax.open("POST", "/Home/DecreaseQuantity");

    ajax.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");


    ajax.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                return this.responseText;
            }
        }
    }
    ajax.send("ProductId=" + productId + "&username=" + username);

}
function deletefromcart(productname,productId, price, username) {
    var total = Number((document.getElementById("total").value).substr(1));
    var quantity = Number(document.getElementById(productId).value);
    var itemcost = price * quantity;
    total -= itemcost;
    document.getElementById("total").value = "$" + total.toString();
    document.getElementById(productname).remove();
    deleteproduct(productId, username);
    if (total == 0) {
        document.getElementById("cart").innerHTML = "Your cart is currently empty.";
    }

}


function deleteproduct(productId, username) {
    let ajax = new XMLHttpRequest();

    ajax.open("POST", "/Home/RemoveProduct");

    ajax.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");


    ajax.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                return this.responseText;
            }
        }
    }
    ajax.send("ProductId=" + productId + "&username=" + username);

}

function submitRating(productId, username, PurchaseDate) {

    var ID = productId.toString();
    var ID = ID.concat(PurchaseDate);
    var section = document.getElementById(ID);
    var ID1 = ID.concat(username);
    var rating = section.options[section.selectedIndex].value;
    var remarks = document.getElementById(ID1);
    if (rating == "0") {
        remarks.innerHTML = "You have not submitted any ratings for this purchase yet.";
    }
    else {
        let output = [];
        // Append all the filled whole stars
        for (var i = rating; i >= 1; i--)
            output.push('<i class="fa fa-star checked"></i>&nbsp;');

        // Fill the empty stars
        for (let i = (5 - rating); i >= 1; i--)
            output.push('<i class="fa fa-star"></i>&nbsp;');

        remarks.innerHTML = "Your rating for this purchase is " + rating + " " + output.join('') + ".If you would like to update your rating.Please resubmit another rating.";

    }
    let ajax = new XMLHttpRequest();

    ajax.open("POST", "/Home/SubmitRating");

    ajax.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");


    ajax.onreadystatechange = function () {
        if (this.readyState == XMLHttpRequest.DONE) {
            if (this.status == 200) {
                return this.responseText;
            }
        }
    }
    ajax.send("rating=" + rating + "&ProductId=" + productId + "&username=" + username + "&purchaseDate=" + PurchaseDate);

}

