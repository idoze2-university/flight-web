var server = 'https://rony3.atwebpages.com';
let flightList = []

//Get all flights from server, internal and external.
function getFlights() {
    const time = new Date().toISOString();
    // const url = server + "/api/Flights?relative_to=" + time + "&sync_all";
    const url = 'http://rony3.atwebpages.com/api/Flights?relative_to=2020-12-26T23:56:21Z&sync_all';
    console.log(url);
    $.getJSON(url, function (data) {
        debugger
        console.log(data);
        flightList = data;
    });
};

function showFlightList() {
    const ul = document.createElement("ul");
    // ul.classList.add("exflights");
    flightList.forEach((flight) => {
        const li = document.createElement("li");
        if (selectedFlightID === flight.flightID) {
            li.classList.add('selected');
        }
        let curflightID = flight.flightID;
        if (classFlightList === ".exflight-list") {
            li.innerHTML = `${flight.flightID} - ${flight.company_name}`;
        } else {
            li.innerHTML = `${flight.flightID} - ${flight.company_name} <a onclick="deleteflightAfterPressingX('${curflightID}');" href="#">X</a>`;
        }
        li.id = flight.flightID;
        ul.append(li);
    });
    $(classFlightList).append(ul);
}

//Get specific details for flightplan
async function getFlightPlan(id) {
    const url = server + "/api/Flights?relative_to=" + time + "&sync_all";
    return await $.getJSON(url);
}

getFlights();
console.log(flightList);

