$(document).ready(function () {
    $(document).on("click", "#registrationForm", function (event) {
        event.preventDefault(); // Prevent default form submission

        var email = $('#Email').val();
        var password = $('#Password').val();
        var firstName = $('#FirstName').val();
        var lastName = $('#LastName').val();

        // Validate email format
        if (!validateEmail(email)) {
            alert('Please enter a valid email address.');
            return;
        }

        // Validate password complexity
        if (!validatePassword(password)) {
            alert('Password must contain at least one uppercase letter, one lowercase letter, one number, one special character, and be at least 8 characters long.');
            return;
        }

        var formData = {
            Email: email,
            Password: password,
            FirstName: firstName,
            LastName: lastName
        };

        $.ajax({
            url: '/User/RegisterUser',
            type: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(formData), 
            success: function (response) {
                alert('User registered successfully!');
                window.location.reload();
            },
            error: function (xhr, status, error) {
                console.error(xhr.responseText);
                alert('User registration failed!');
            }
        });
    });

});

    

    // Function to validate email format
    function validateEmail(email) {
        var re = /\S+@\S+\.\S+/;
        return re.test(email);
    }

    // Function to validate password complexity
    function validatePassword(password) {
        var re = /^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[!@#$%^&*()_+])(?=.*[a-zA-Z]).{8,}$/;
        return re.test(password);
    }