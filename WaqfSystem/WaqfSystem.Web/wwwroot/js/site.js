// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// SignalR Real-time Notifications configuration
const missionConn = new signalR.HubConnectionBuilder()
    .withUrl("/missionHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

missionConn.on("ReceiveMissionUpdate", (id, title, status, msg) => {
    // Basic toast / alert integration
    if (typeof showAlert === 'function') {
        showAlert(msg, 'info');
    } else {
        console.log(`Mission Update [${id}]: ${title} -> ${status} (${msg})`);
        // If there's a badge, update it or refresh the partial
        location.reload(); // Simple refresh for now to update the partial count
    }
});

missionConn.on("ReceiveAssignment", (id, missionCode, title) => {
    if (typeof showAlert === 'function') {
        showAlert(`تم تكليفك بمهمة جديدة: ${missionCode} - ${title}`, 'success');
    } else {
        alert(`تم تكليفك بمهمة جديدة: ${missionCode} - ${title}`);
        location.reload();
    }
});

missionConn.start().catch(err => console.error(err.toString()));

function showAlert(message, type = 'info') {
    const alertDiv = document.createElement('div');
    alertDiv.className = `alert alert-${type} alert-dismissible fade show fixed-top m-3 shadow-lg`;
    alertDiv.role = 'alert';
    alertDiv.style.zIndex = '9999';
    alertDiv.style.width = '350px';
    alertDiv.style.right = '0';
    alertDiv.style.left = 'unset';
    alertDiv.innerHTML = `
        <div class="d-flex align-items-center">
            <i class="fas fa-bell me-2"></i>
            <div>${message}</div>
        </div>
        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close"></button>
    `;
    document.body.appendChild(alertDiv);
    setTimeout(() => {
        $(alertDiv).alert('close');
    }, 5000);
}

// Write your JavaScript code.
