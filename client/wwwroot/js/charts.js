// Chart.js interop functions for Blazor
const chartInstances = {};

function destroyChart(canvasId) {
    if (chartInstances[canvasId]) {
        chartInstances[canvasId].destroy();
        delete chartInstances[canvasId];
    }
}

window.renderBarChart = function (canvasId, labels, data, label) {
    destroyChart(canvasId);
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    chartInstances[canvasId] = new Chart(ctx, {
        type: 'bar',
        data: {
            labels: labels,
            datasets: [{
                label: label || 'Hours Studied',
                data: data,
                backgroundColor: 'rgba(27, 110, 194, 0.7)',
                borderColor: 'rgba(27, 110, 194, 1)',
                borderWidth: 1,
                borderRadius: 4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            return context.parsed.y.toFixed(1) + ' hrs';
                        }
                    }
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    title: { display: true, text: 'Hours' }
                },
                x: {
                    ticks: {
                        maxRotation: 45,
                        autoSkip: true,
                        maxTicksLimit: window.innerWidth < 768 ? 7 : 15
                    }
                }
            }
        }
    });
};

window.renderDonutChart = function (canvasId, labels, data) {
    destroyChart(canvasId);
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    const colors = [
        '#1b6ec2', '#28a745', '#ffc107', '#dc3545', '#6f42c1',
        '#17a2b8', '#fd7e14', '#20c997', '#e83e8c', '#6c757d'
    ];

    chartInstances[canvasId] = new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: labels,
            datasets: [{
                data: data,
                backgroundColor: colors.slice(0, labels.length),
                borderWidth: 2,
                borderColor: '#fff'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { padding: 16 }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            const total = context.dataset.data.reduce((a, b) => a + b, 0);
                            const pct = ((context.parsed / total) * 100).toFixed(1);
                            return context.label + ': ' + context.parsed.toFixed(1) + ' hrs (' + pct + '%)';
                        }
                    }
                }
            }
        }
    });
};

window.renderLineChart = function (canvasId, labels, datasets) {
    destroyChart(canvasId);
    const ctx = document.getElementById(canvasId);
    if (!ctx) return;

    const colors = [
        '#1b6ec2', '#28a745', '#ffc107', '#dc3545', '#6f42c1'
    ];

    const chartDatasets = [];
    for (let i = 0; i < datasets.length; i++) {
        const ds = datasets[i];
        const color = colors[i % colors.length];

        // Convert -1 sentinel to null so Chart.js skips those points
        const progressData = ds.points.map(p => p.cumulativeHours < 0 ? null : p.cumulativeHours);

        // Progress line
        chartDatasets.push({
            label: ds.goalName,
            data: progressData,
            borderColor: color,
            backgroundColor: color + '20',
            fill: false,
            tension: 0.3,
            pointRadius: 3,
            spanGaps: false
        });

        // Target reference line — only span the range where this goal has data
        if (ds.targetHours > 0) {
            const targetData = ds.points.map(p => p.cumulativeHours < 0 ? null : ds.targetHours);
            chartDatasets.push({
                label: ds.goalName + ' Target',
                data: targetData,
                borderColor: color,
                borderDash: [5, 5],
                pointRadius: 0,
                fill: false,
                spanGaps: true
            });
        }
    }

    chartInstances[canvasId] = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: chartDatasets
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    position: 'bottom',
                    labels: { padding: 16 }
                },
                tooltip: {
                    callbacks: {
                        label: function (context) {
                            if (context.parsed.y == null) return null;
                            return context.dataset.label + ': ' + context.parsed.y.toFixed(1) + ' hrs';
                        }
                    }
                }
            },
            scales: {
                x: {
                    ticks: {
                        maxRotation: 45,
                        autoSkip: true,
                        maxTicksLimit: window.innerWidth < 768 ? 7 : 15
                    }
                },
                y: {
                    beginAtZero: true,
                    title: { display: true, text: 'Hours' }
                }
            }
        }
    });
};
