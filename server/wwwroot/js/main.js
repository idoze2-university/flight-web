

// const server = 'http://rony3.atwebpages.com'; // TO-DO: change to local server
const server = '';
let flightList = {};
let flightPlans = {};
let flightMarkers = {};
let selectedId = "";
let map = 0;
let selectedFlightPath = 0;

function main() {
	console.log("Starting main.js...");
	initDragAndDrop();
	renderFlightDetails();
	renderFlightList();
	getFlights();
}

//Get all flights from API, internal and external.
async function getFlights() {
	const header_type = 'h3';
	const header = document.createElement(header_type);
	header.innerHTML = "Scanning for flights...";
	$('#fw-board-in').append(header);
	const time = new Date().toISOString().split('.')[0] + "Z"; // TO-DO: change to const
	// time = "2020-12-26T23:56:21Z"; // TO-DO: remove
	const url = server + "/api/Flights?relative_to=" + time + "&sync_all";
	console.log("getFlights(): fetching flights from " + url);
	await $.getJSON(url, function (data) {
		flightList = {};
		for (const key in data) {
			const flight = data[key];
			const id = flight.flight_id;
			flightList[id] = flight;
		}
	}).done(function () {
		if (Object.keys(flightList).length > 0) {
			console.log("getFlights(): fetched " + Object.keys(flightList).length + " flights.");
		}
		else {
			console.log("getFlights(): fetched " + 0 + " flights.")
		}
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

function refreshFlightList() {
	getFlights();
}

async function postFlight(f) {
	const url = server + "/api/FlightPlan";
	console.log(`postFlight(): posting a new flight`);
	await $.ajax(
		{
			url: url,
			data: JSON.stringify(f),
			contentType: 'application/json',
			type: 'POST',
			dataType: 'json',
			processData: false,
			success: function (data) {
				console.log(`postFlight(): successfuly created flight '${data}'`);
			},
		});
	refreshFlightList();
}

async function deleteFlight(id) {
	const url = server + "/api/Flights/" + id;
	console.log(`deleteFlight(): deleting flight '${id}' `);
	$(`#FL-${id}`).addClass('disabled');
	$(`#DEL-${id}`).addClass('disabled');
	clearFlightDetails();
	await $.ajax(
		{
			url: url,
			type: 'DELETE',
			success: function (data) {
				console.log(`deleteFlight(): successfuly deleted flight '${id}'`);
				removeFlight(id);
				removeMarker(id);
				initMap();
				getFlights();
			},
		});
}


function clearFlightList() {
	console.log("clearFlightList(): clearing list");
	$("#fw-board-in").empty();
	$("#fw-board-ex").empty();
}
//Renders the flight (in and ex) list.
function renderFlightList() {
	clearFlightList();
	if (Object.keys(flightList).length > 0) {
		const internalList = document.createElement('ul');
		internalList.className = "list-group";
		const externalList = document.createElement('ul');
		externalList.className = "list-group";
		for (const key in flightList) {
			const fl = flightList[key];
			const item = document.createElement('li');
			const id = fl.flight_id;
			getFlightPlan(id).then(() => {
				addFlightMarker(id);
			});
			item.className = "fl-item list-group-item list-group-item-action primary disabled";
			item.id = "FL-" + id;
			item.innerHTML = ` <ul class="list-inline m-0" style="text-align:center;">
		<li class="list-inline-item">${id} | ${fl.company_name}</li>
		`;
			item.innerHTML += fl.is_external ? '' : `<li class="list-inline-item"><button class="btn btn-danger btn-sm rounded-0 disabled" id="DEL-${id}" type="button" data-placement="top" title="Delete">X</button></li>`;
			(fl.is_external ? externalList : internalList).append(item);
			//TO-DO: Test external flights UI.
		}
		const header_type = 'h3';
		if (internalList.children.length > 0) {
			let header = document.createElement(header_type);
			header.innerHTML = "Internal Flights:";
			$('#fw-board-in').append(header);
			$('#fw-board-in').append(internalList);
		}
		if (externalList.children.length > 0) {
			const header = document.createElement(header_type);
			header.innerHTML = "External Flights:";
			$('#fw-board-ex').append(header);
			$('#fw-board-ex').append(externalList);
		}
	}
	else {
		const header_type = 'h3';
		const header = document.createElement(header_type);
		header.innerHTML = "No Flights Found.";
		$('#fw-board-in').append(header);
	}

}

function clearFlightDetails() {
	console.log("clearFlightDetails(): clearing details");
	const details = $('#fw-details');
	details.empty();
}
function renderFlightDetails() {
	clearFlightDetails();
	if (selectedId != "") {
		renderSelectedFlightPath();
		console.log(`renderFlightDetails(): showing details for '${selectedId}'`);
		const detailsBox = $('#fw-details');
		const table = document.createElement("table");
		table.id = "fd-table";
		table.className = "table";
		table.style = "text-align:center;";
		table.innerHTML = `
	<thead>
    <tr id ="fd-top"></tr>
	</thead>
	<tbody>
	<tr id="fd-details"></tr>
	</tbody>
	`
		detailsBox.append(table);
		const fd_top = $("#fd-top");
		const fd_details = $("#fd-details");
		const plan = flightPlans[selectedId];
		const details = generatePlanDetails(plan)
		//Add values as table;
		for (var item of details) {
			const key = item['name']; const val = item['value'];
			const key_elem = document.createElement("th");
			key_elem.scope = "row";
			key_elem.innerHTML = key;
			const val_elem = document.createElement("td");
			val_elem.scope = "row";
			val_elem.innerHTML = val;
			fd_top.append(key_elem);
			fd_details.append(val_elem);
		}
	}
	else {
		const detailsBox = $('#fw-details');
		const msg = document.createElement("h3");
		msg.style = "text-align:center;";
		msg.innerHTML = "Please select a flight from the list or the map.";
		detailsBox.append(msg);
	}
}

function generatePlanDetails(plan) {
	const start_p = plan.initial_location;
	const end_p = plan.segments[plan.segments.length - 1];
	//calculate the required details and match to keys in dict array
	const details = [
		{
			name: "ID",
			value: selectedId
		},
		{
			name: "Company",
			value: plan.company_name
		},
		{
			name: "# Passengers",
			value: plan.passengers
		},
		{
			name: "Start Point",
			value: `${start_p.longitude}/${start_p.latitude}`
		},
		{
			name: "End Point",
			value: `${end_p.longitude}/${end_p.latitude}`
		},
	]
	return details;
}

function removeFlight(id) {
	$(`#FL-${id}`).remove();
	deSelectFlight();
}

//Logic Functions ######
function deSelectFlight() {
	if (selectedId != "") {
		try {
			$('#FL-' + selectedId).removeClass('active');
			console.log(`deSelectFlight(): deselected flight ${selectedId}`);
			clearSelectedFlightPath();
			deselectMarker(selectedId);
		}
		finally {
			selectedId = "";
		}
	}
	renderFlightDetails();
}

function selectFlight(id) {
	const current_selected = selectedId;
	deSelectFlight();
	if (id != current_selected) {
		selectedId = id;
		console.log(`selectFlight(): selected flight ${selectedId}`);
		$('#FL-' + selectedId).addClass('active');
		selectMarker(id);
	}
	renderFlightDetails();
}


//Map functions ######
// API KEY : AIzaSyAuJs-6qtB_AgqXqO2ScPtE5W9RumFDelg

function initMap() {
	$("#map_elem").remove();
	const map_elem = document.createElement("div");
	map_elem.id = "map_elem";
	const options = {
		zoom: 3,
		minZoom: 1,
		center: { lat: 10, lng: 10 },
	};
	map = new google.maps.Map(map_elem, options);
	for (const markerKey in flightMarkers) {
		const marker = flightMarkers[markerKey];
		marker.setMap(map);
	}
	$("#fw-map").append(map_elem);
}

function addFlightMarker(id) {
	const fl = flightList[id];
	const plan = flightPlans[id];

	const icon = {
		url: "./common/airplane.png", // url
		scaledSize: new google.maps.Size(60, 60), // scaled size
		origin: new google.maps.Point(0, 0), // origin
		anchor: new google.maps.Point(30, 30) // anchor
	};

	const marker = new google.maps.Marker({

		position: { lat: parseFloat(fl.latitude), lng: parseFloat(fl.longitude) },
		icon: icon,
		map: map
	});
	flightMarkers[id] = marker;

	marker.addListener("click", function () {
		selectFlight(id);
	})
}

function selectMarker(id) {
	const marker = flightMarkers[id];
	const icon = {
		url: "./common/airplane-full.png", // url
		scaledSize: new google.maps.Size(60, 60), // scaled size
		origin: new google.maps.Point(0, 0), // origin
		anchor: new google.maps.Point(30, 30) // anchor
	};
	marker.setIcon(icon);

}

function deselectMarker(id) {
	const marker = flightMarkers[id];
	const icon = {
		url: "./common/airplane.png", // url
		scaledSize: new google.maps.Size(60, 60), // scaled size
		origin: new google.maps.Point(0, 0), // origin
		anchor: new google.maps.Point(30, 30) // anchor
	};
	marker.setIcon(icon);

}

function removeMarker(id) {
	const marker = flightMarkers[id];
	marker.setMap(null);
	delete flightMarkers[id];
}

function renderSelectedFlightPath() {
	const plan = flightPlans[selectedId];
	const path = [
		{
			lat: parseFloat(plan.initial_location.latitude),
			lng: parseFloat(plan.initial_location.longitude)
		}
	];
	for (const key in plan.segments) {
		const segment = plan.segments[key];
		path.push(
			{
				lat: parseFloat(segment.latitude),
				lng: parseFloat(segment.longitude)
			}
		)
	}
	console.log(path);
	selectedFlightPath = new google.maps.Polyline({
		path: path,
		geodesic: true,
		strokeColor: '#FF0000',
		strokeOpacity: 1.0,
		strokeWeight: 2
	});
	selectedFlightPath.setMap(map);
}

function clearSelectedFlightPath() {
	if (selectedFlightPath != 0) {
		selectedFlightPath.setMap(null);
	}
	selectedFlightPath = 0;
}

function initDragAndDrop() {
	function handleDrag(e) {
		$("#fw-upload").addClass('drop');
		e.stopPropagation();
		e.preventDefault();
		e.dataTransfer.dropEffect = 'copy'; // Explicitly show this is a copy.
	}

	function handleDrop(e) {
		$("#fw-upload").removeClass('drop');
		e.stopPropagation();
		e.preventDefault();
		let json_file = e.dataTransfer.files[0];
		if (json_file.type.match('application/json')) {
			var reader = new FileReader();
			reader.onload = (function (theFile) {
				return function (e) {
					const parsed_json = JSON.parse(e.target.result);
					postFlight(parsed_json);
				};
			})(json_file);
			reader.readAsText(json_file);
		}
		else {
			alert("Please upload a valid JSON File!");
		}
	}

	let dropZone = document.getElementById('fw-upload');
	dropZone.addEventListener('dragover', handleDrag, false);
	dropZone.addEventListener('draglen promiseave', function (evt) { $("#fw-upload").removeClass('drop'); }, false);
	dropZone.addEventListener('drop', handleDrop, false);

	let doc = document.getElementsByTagName("html")[0];
	doc.addEventListener('drop', function (e) {
		e.stopPropagation();
		e.preventDefault();
	}, false);
}

$(document).ready(function () {
	main();
});
