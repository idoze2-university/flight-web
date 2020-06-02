const server = 'http://rony3.atwebpages.com';
let flightList = [];
let flightPlans = {};
let selectedFlight = "";

//Get all flights from API, internal and external.
async function getFlights() {
	let time = new Date().toISOString();
	time = "2020-12-26T23:56:21Z"; //for debugging
	const url = server + "/api/Flights?relative_to=" + time + "&sync_all";
	console.log("getFlights(): fetching flights from " + url);
	await $.getJSON(url, function (data) {
		console.log("getFlights(): fetched " + data.length + " flights.");
		flightList = data;
	}).done(function () {
		renderFlightList();
	});
};

//Get specific details for flightplan by ID from API.
async function getFlightPlan(id) {
	const url = server + "/api/FlightPlan/" + id;
	console.log("getFlightPlan(): fetching plan for \'" + id + "\' from " + url);
	await $.getJSON(url, function (data) {
		flightPlans[id] = data;
		console.log("getFlightPlan(): fetched plan for \'" + id + "\': " + flightPlans[id]);
		let item = $('#FL-' + id);
		item.removeClass('disabled');
		$(item).on('click', { ID: id }, (event) => {
			selectFlight(event.data.ID);
		});
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
		let id = fl.flight_id;
		getFlightPlan(id)
		item.className = "fl-item list-group-item list-group-item-action primary disabled";
		item.id = "FL-" + id;
		if (fl.is_external) {
			item.innerHTML = `${id} | ${fl.company_name}`;
			externalList.append(item);
		} else {
			// item.innerHTML = `${id} | ${fl.company_name} <button type="button" class="btn btn-danger" on-click="deleteFlight(${id})">X</button>`;
			item.innerHTML = `${id} | ${fl.company_name}`;
			internalList.append(item);
		}
	});
	if (internalList.children.length > 0) {
		let header = document.createElement('h2');
		header.innerHTML = "Internal Flights:";
		$('#fw-board-in').append(header);
		$('#fw-board-in').append(internalList);
	}
	if (externalList.children.length > 0) {
		let header = document.createElement('h2'); internalList
		header.innerHTML = "External Flights:";
		$('#fw-board-ex').append(header);
		$('#fw-board-ex').append(externalList);
	}
}

function renderFlightDetails() {
	const detailsBox = $('#fw-details');
}

function deSelectFlight() {
	if (selectFlight != "") {
		$('#FL-' + selectedFlight).removeClass('active');
	}
}

function selectFlight(id) {
	deSelectFlight();
	selectedFlight = id;
	$('#FL-' + selectedFlight).addClass('active');
	console.log(flightPlans[id]);
}
getFlights();

