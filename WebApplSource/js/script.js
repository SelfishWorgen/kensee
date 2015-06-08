var password_strength = 0;

$(document).ready(function() {
	$(document).on('focus', '#password', function() {
		$('.password_strength').show();										  
	});
	
	$(document).on('focusout', '#password', function() {
		$('.password_strength').hide();									  
	});
	
	$(document).on('keyup', '#password', function() {
		checkStrength($('#password').val());					  
	});	
});

function checkStrength(password){
		
		$('.font-color').each(function() {
			$(this).removeClass('font-color');
		});
		
		//initial strength
		var strength = password_strength = 0;
		
		//if the password length is less than 6, return message.
		if (password.length < 6) { 
			$('.bad').addClass('font-color')
			return false
		}
		
		//length is ok, lets continue.
		
		//if length is 8 characters or more, increase strength value
		if (password.length >= 6) strength += 1
		
		//if password contains both lower and uppercase characters, increase strength value
		if (password.length >= 6 && password.match(/([a-z].*[A-Z])|([A-Z].*[a-z])/))  strength += 1
		
		//if it has numbers and characters, increase strength value
		if (password.length >= 6 && password.match(/([a-zA-Z])/) && password.match(/([0-9])/))  strength += 1 
		
		//if it has one special character, increase strength value
		if (password.length >= 6 && password.match(/([!,%,&,@,#,$,^,*,?,_,~])/))  strength += 1
		
		//if it has two special characters, increase strength value
		if (password.length >= 6 &&password.match(/(.*[!,%,&,@,#,$,^,*,?,_,~].*[!,%,&,@,#,$,^,*,?,_,~])/)) strength += 1
		
		//now we have calculated strength value, we can return messages
		
		password_strength = strength;
		
		//if value is less than 2
		if (strength && strength < 2) {
			$('.bad').removeClass('font-color')
			$('.weak').addClass('font-color')
			return false		
		} else if (strength >= 2 ) {
			$('.weak').removeClass('font-color')
			$('.strong').addClass('font-color')
			return false	
		}
		
	}