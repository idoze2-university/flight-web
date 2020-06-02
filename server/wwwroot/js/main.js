const server = 'http://rony3.atwebpages.com'; // TODO: change to local server
let flightList = [];
let flightPlans = {};
let selectedFlight = "";

//Get all flights from API, internal and external.
async function getFlights() {
	let time = new Date().toISOString(); // TODO: change to const
	time = "2020-12-26T23:56:21Z"; // TODO: remove
	const url = server + "/api/Flights?relative_to=" + time + "&sync_all";
	console.log("getFlights(): fetching flights from " + url);
	await $.getJSON(url, function (data) {
		flightList = data;
	}).done(function () {
		if (Array.isArray(flightList)) {
			console.log("getFlights(): fetched " + flightList.length + " flights.");
			renderFlightList();
		}
		else {
			console.log("getFlights(): fetched " + 0 + " flights.")
		}
	});
};

//Get specific details for flightplan by ID from API.
async function getFlightPlan(id) {
	const url = server + "/api/FlightPlan/" + id;
	console.log("getFlightPlan(): fetching plan for \'" + id + "\' from " + url);
	await $.getJSON(url, function (data) {
		flightPlans[id] = data;
		console.log("getFlightPlan(): fetched plan for \'" + id + "\': " + flightPlans[id]);

		//Make flight selectable
		const item = $(`#FL-${id}`);
		item.removeClass('disabled');
		$(item).on('click', { ID: id }, (event) => {
			selectFlight(event.data.ID);
		});

		//Make flight deletable
		const item_del_btn = $(`#DEL-${id}`);
		item_del_btn.removeClass('disabled');
		$(item_del_btn).on('click', { ID: id }, (event) => {
			deleteFlight(id);
		});
	});
}

async function deleteFlight(id) {
	const url = server + "/api/Flights/" + id;
	console.log(`deleteFlight(): deleting flight '${id}' `);
	await $.ajax(
		{
			url: url,
			type: 'DELETE',
			success: function (data) {
				console.log(`deleteFlight(): successfuly deleted flight '${id}'`);
				removeFlight(id);
			},
		});
}

//Renders the flight (in and ex) list.
function renderFlightList() {
	const internalList = document.createElement('ul');
	internalList.className = "list-group";
	const externalList = document.createElement('ul');
	externalList.className = "list-group";
	flightList.forEach((fl) => {
		const item = document.createElement('li');
		const id = fl.flight_id;
		getFlightPlan(id)
		item.className = "fl-item list-group-item list-group-item-action primary disabled";
		item.id = "FL-" + id;
		item.innerHTML = ` <ul class="list-inline m-0" style="text-align:center;">
		<li class="list-inline-item">${id} | ${fl.company_name}</li>
		`;
		item.innerHTML += fl.is_external ? '' : `<li class="list-inline-item"><button class="btn btn-danger btn-sm rounded-0 disabled" id="DEL-${id}" type="button" data-placement="top" title="Delete">X</button></li>`;
		(fl.is_external ? externalList : internalList).append(item);
	});
	const header_type = 'h3';
	if (internalList.children.length > 0) {
		let header = document.createElement(header_type);
		header.innerHTML = "Internal Flights:";
		$('#fw-board-in').append(header);
		$('#fw-board-in').append(internalList);
	}
	if (externalList.children.length > 0) {
		let header = document.createElement(header_type);
		header.innerHTML = "External Flights:";
		$('#fw-board-ex').append(header);
		$('#fw-board-ex').append(externalList);
	}
}

function renderFlightDetails() {
	const detailsBox = $('#fw-details');
	const fd_id = document.createElement("div");
	fd_id.className = "";
	detailsBox.append();
}

function removeFlight(id) {
	$(`#FL-${id}`).remove();
	deSelectFlight();
}


//Logic Functions ######
function deSelectFlight() {
	if (selectedFlight != "") {
		try {
			$('#FL-' + selectedFlight).removeClass('active');
			console.log(`deSelectFlight(): deselected flight ${selectedFlight}`);
		}
		finally {
			selectedFlight = "";
		}
	}
}

function selectFlight(id) {
	deSelectFlight();
	selectedFlight = id;
	$('#FL-' + selectedFlight).addClass('active');
	renderFlightDetails();
}
getFlights();

