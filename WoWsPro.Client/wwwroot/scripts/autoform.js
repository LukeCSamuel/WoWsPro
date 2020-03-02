window.autoform = {
	submitOpenId: (address, body) => {
		var form = document.createElement('form');
		form.setAttribute('method', 'post');
		form.setAttribute('action', address);

		for (var key in body) {
			var field = document.createElement('input');
			field.setAttribute('type', 'hidden');
			field.setAttribute('name', 'openid.' + key);
			field.setAttribute('value', body[key]);

			form.appendChild(field);
		}

		document.body.appendChild(form);
		form.submit();
	},
	submitOauth2: (address, body) => {
		var form = document.createElement('form');
		form.setAttribute('method', 'get');
		form.setAttribute('action', address);

		for (var key in body) {
			var field = document.createElement('input');
			field.setAttribute('type', 'hidden');
			field.setAttribute('name', key);
			field.setAttribute('value', body[key]);

			form.appendChild(field);
		}

		document.body.appendChild(form);
		form.submit();
    }
}