var numErrors = 0;
$(document).ready(function() {
	jQuery.validator.addMethod("passwordStrength", function(value, element) {
		checkStrength($(element).val());
		return password_strength;
	}, "Please specify a more secure password.");
	
	$('#what .dropdown1-menu a').click(function() {
		var search_by = $(this).attr('rel');
		if (search_by == '')
			search_by = 'Title / ISBN / Author / EAN';
		var selected_text = $(this).html();
		$('input[name="search_by"]').val(search_by);
		$('input[name="q"]').attr('placeholder', capitaliseFirstLetter(search_by));
		//$('input[name="q"]').val('');
		$(this).parent().parent().parent().find('.dropdown1-toggle').html(selected_text+' <b class="caret"></b>');
		$(this).parents('.open').removeClass('open');
		return false;
	});
	
	$(".ajax-modal").live('click', function() {
		if (!modal_visible)
			$('#modal').modal('show');
		else
			$('#modal').html('<div style="padding:10px; text-align:center;"><img src="'+site_url+'img/loading.gif" /> Loading...</div>');
		var url = $(this).attr('href');
		$('#modal').load(url+'?mode=modal', function() {
			validation();
			$('#modal').modal('show');
		});
		
		return false;
	 });
	 
	 $(".reviews_modal").live('click', function() {
		var url = $(this).attr('href');
		var found = url.indexOf("?");
		
		$('#generic_modal').modal('show');
		
		if (found > 0)
			url = url+'&';
		else
			url = url+'?';
			
		var modal_id = '#generic_modal';
		$(modal_id).load(url+'mode=modal', function() {
			
		});
		
		return false;
	 });
	 
	$('#modal').on('hidden', function () {
		modal_visible = false;
		$('.wait_message').remove();
		$('#modal').html('<div style="padding:10px; text-align:center;"><img src="'+site_url+'img/loading.gif" /> Loading...</div>');
	});
	$('#modal').on('shown', function () {
		modal_visible = true;
	});
});
function validation() {
	$('.alert').remove();
	
	$('#login_email_form').validate({
		highlight: function(element, errorClass, validClass) {
			$(element).addClass('field_error');
			if($(element).data("hightlighed") == "1") {		

			} else {				
				$(element).data("hightlighed", 1);
				numErrors++;
			}

			$("#required-note").css("display", "block");
		},
		unhighlight: function(element, errorClass, validClass) {
			$(element).removeClass('field_error');

			$('.input_'+$(element).attr('name')).remove();

			if($(element).data("hightlighed") == "1") {
				$(element).data("hightlighed", 0);
				numErrors--;
			}

		},
		errorPlacement: function(error, element) {
			if (!$('#input_'+$(element).attr('name')).length)
				$('.modal-body').before('<div class="alert alert-error input_'+$(element).attr('name')+'" id="input_'+$(element).attr('name')+'">'+error.html()+'</div>');
	    },		
		submitHandler: function() {
			$('.alert').remove();
			jQuery.post(site_url+'process/validateLogin.php', $("#login_email_form").serialize(), function(data) {
				if (data != '') {
					$('.modal-body').before('<div class="alert alert-error">'+data+'</div>');
				} else {
					location.reload();
				}
			});
		}, 
		rules: {
			password: "required",
			email: {
				required: true,
				email: true
			},
		},
		messages: {
			email: "Please enter your Email",
			password: "Please enter your password."
		}
	});
	$('#signup_form').validate({
		highlight: function(element, errorClass, validClass) {
			$(element).addClass('field_error');
			if($(element).data("hightlighed") == "1") {		

			} else {				
				$(element).data("hightlighed", 1);
				numErrors++;
			}

			$("#required-note").css("display", "block");
		},
		unhighlight: function(element, errorClass, validClass) {
			$(element).removeClass('field_error');
			
			$('.input_'+$(element).attr('name')).remove();

			if($(element).data("hightlighed") == "1") {
				$(element).data("hightlighed", 0);
				numErrors--;
			}

		},
		errorPlacement: function(error, element) {			
			if (!$('#input_'+$(element).attr('name')).length)
				$('.modal-body').before('<div class="alert alert-error input_'+$(element).attr('name')+'" id="input_'+$(element).attr('name')+'">'+error.html()+'</div>');
	    },		
		submitHandler: function() {
			$('.alert').remove();
			jQuery.post(site_url+'process/validateSignup.php', $("#signup_form").serialize(), function(data) {
				if (data != '') {
					$('.modal-body').before('<div class="alert alert-error">'+data+'</div>');
				} else {
					location.reload();
				}
			});
		}, 
		rules: {
			password: {
				required: true,
				passwordStrength: true
			},
			captcha: "required",
			email: {
				required: true,
				email: true
			},
		},
		messages: {
			email: "Please enter your Email",
			password: {
				required: "Please enter your password.",
				passwordStrength: "Please specify a more secure password."
			},
			captcha: "Please enter the security code"
		}
	});
	$('#reset_email_form').validate({
		highlight: function(element, errorClass, validClass) {
			$(element).addClass('field_error');
			if($(element).data("hightlighed") == "1") {		

			} else {				
				$(element).data("hightlighed", 1);
				numErrors++;
			}

			$("#required-note").css("display", "block");
		},
		unhighlight: function(element, errorClass, validClass) {
			$(element).removeClass('field_error');
			
			$('.input_'+$(element).attr('name')).remove();

			if($(element).data("hightlighed") == "1") {
				$(element).data("hightlighed", 0);
				numErrors--;
			}

		},
		errorPlacement: function(error, element) {			
			if (!$('#input_'+$(element).attr('name')).length)
				$('.modal-body').before('<div class="alert alert-error input_'+$(element).attr('name')+'" id="input_'+$(element).attr('name')+'">'+error.html()+'</div>');
	    },	
		submitHandler: function() {
			$('.alert').remove();
			jQuery.post(site_url+'process/validateForgot.php', $("#reset_email_form").serialize(), function(data) {
				if (data != '') {
					$('.modal-body').before('<div class="alert alert-error">'+data+'</div>');
				} else {
					$('.modal-body').html('');
					$('.modal-body').append('<div class="alert alert-success">A confirmation email has been sent to your email address. Please click on the link in the email to activate the password request for your OO.sg account.</div><p class="wait_message">Please wait <span class="countdown_timer" rel="300">5:00</span> mins before requesting again.</p>');					
					countdown(300);
				}
			});
		},
		rules: {
			email: {
				required: true,
				email: true
			},
			captcha: "required"
		},
		messages: {
			email: "Please enter your email",
			captcha: "Please enter the security code"
		}
	});
	
	if ($('.countdown_timer').length && $('.countdown_timer').attr('rel')) {
		clearTimeout(timeoutEvent);
		countdown(parseInt($('.countdown_timer').attr('rel')));
	}
}

function capitaliseFirstLetter(string)
{
	return string.charAt(0).toUpperCase() + string.slice(1);
}

var secs = 0;
var timeoutEvent;
function countdown(seconds) {
secs = seconds;
timeoutEvent = setTimeout('Decrement()',1000);
}
function Decrement() {
	// if less than a minute remaining
	if (secs < 59) {
		var m = '0';
		var s = secs;
		if (s < 10)
			s = '0'+s;
	} else {
		var m = getminutes();
		var s = getseconds();
	}
	$('.countdown_timer').html(m+':'+s);

	secs--;
	
	if (secs < 0) {
		$('.wait_message').html('<a href="'+site_url+'forgot_password.html" class="ajax-modal">Resend confirmation email</a>');
		$('.already_sent_message').html('Enter you account\'s email address and the Recaptcha below, and we will send you a confirmation email shortly.');
		$('.newStylishButton').removeAttr('disabled');
		return false;
	}
		
	timeoutEvent = setTimeout('Decrement()',1000);
}
function getminutes() {
	// minutes is seconds divided by 60, rounded down
	mins = Math.floor(secs / 60);
	return mins;
}
function getseconds() {
	// take mins remaining (as seconds) away from total seconds remaining
	var s = secs-Math.round(mins *60);
	if (s < 10)
		return '0'+s;
	else
		return s;
}