window.managementTransactions = (() => {
    const requestIdentifierHeader = "X-GD92-Management-Request-Id";
    const pendingRequests = new Map();
    const pendingObservations = new Map();
    const normalizeRequestIdentifier = identifier => identifier.replaceAll("-", "");
    let startingConnection;

    const connection = new signalR.HubConnectionBuilder()
        .withUrl("/hubs/management-transactions")
        .withAutomaticReconnect()
        .build();

    const startConnection = async () => {
        if (connection.state === signalR.HubConnectionState.Connected) {
            return;
        }

        if (startingConnection === undefined) {
            startingConnection = connection.start()
                .finally(() => {
                    startingConnection = undefined;
                });
        }

        await startingConnection;
    };

    const complete = async request => {
        if (request.statusUrl === undefined || request.completing) {
            return;
        }

        request.completing = true;
        try {
            const response = await fetch(request.statusUrl);
            if (!response.ok) {
                throw new Error("The Management Transaction status could not be retrieved.");
            }

            const status = await response.json();
            if (status.state === "pending" || status.state === "deferred") {
                return;
            }

            pendingRequests.delete(request.identifier);
            request.resolve(status);
        } catch (error) {
            pendingRequests.delete(request.identifier);
            request.reject(error);
        } finally {
            request.completing = false;
        }
    };

    connection.on("TransactionCompleted", notification => {
        const request = pendingRequests.get(normalizeRequestIdentifier(notification.requestIdentifier));
        if (request === undefined) {
            return;
        }

        request.completed = true;
        void complete(request);
    });

    const updateObservation = (observation, status) => {
        try {
            observation.onStatus(status);
            if (observation.isComplete(status)) {
                pendingObservations.delete(observation.identifier);
                observation.resolve(status);
            }
        } catch (error) {
            pendingObservations.delete(observation.identifier);
            observation.reject(error);
        }
    };

    const refreshObservation = async observation => {
        if (observation.statusUrl === undefined || observation.refreshing) {
            observation.refreshPending = true;
            return;
        }

        observation.refreshing = true;
        observation.refreshPending = false;
        try {
            const response = await fetch(observation.statusUrl);
            if (!response.ok) {
                throw new Error("The Inventory Scan status could not be retrieved.");
            }

            const status = await response.json();
            updateObservation(observation, status);
        } catch (error) {
            pendingObservations.delete(observation.identifier);
            observation.reject(error);
        } finally {
            observation.refreshing = false;
            if (observation.refreshPending &&
                pendingObservations.has(observation.identifier)) {
                void refreshObservation(observation);
            }
        }
    };

    connection.on("InventoryScanUpdated", notification => {
        const observation = pendingObservations.get(
            normalizeRequestIdentifier(notification.requestIdentifier));
        if (observation !== undefined) {
            updateObservation(observation, notification.status);
        }
    });

    const reconcileOutstandingRequests = () => {
        for (const request of pendingRequests.values()) {
            void complete(request);
        }

        for (const observation of pendingObservations.values()) {
            void refreshObservation(observation);
        }
    };

    connection.onreconnected(reconcileOutstandingRequests);

    connection.onclose(() => {
        void reconnect();
    });

    const reconnect = async () => {
        try {
            await startConnection();
            reconcileOutstandingRequests();
        } catch (error) {
            console.error("Could not reconnect to the management transaction hub.", error);
            window.setTimeout(() => {
                void reconnect();
            }, 5000);
        }
    };

    void reconnect();

    return {
        async submit(url, options = {}) {
            await startConnection();

            const identifier = crypto.randomUUID().replaceAll("-", "");
            let resolve;
            let reject;
            const completion = new Promise((resolvePromise, rejectPromise) => {
                resolve = resolvePromise;
                reject = rejectPromise;
            });
            const request = {
                identifier,
                resolve,
                reject,
                statusUrl: undefined,
                completed: false,
                completing: false
            };
            pendingRequests.set(identifier, request);

            try {
                const headers = new Headers(options.headers);
                headers.set(requestIdentifierHeader, identifier);
                const response = await fetch(url, { ...options, headers });
                if (!response.ok || !response.url) {
                    throw new Error("The Management Transaction could not be submitted.");
                }

                request.statusUrl = response.url;
                if (request.completed) {
                    void complete(request);
                }
            } catch (error) {
                pendingRequests.delete(identifier);
                reject(error);
            }

            return completion;
        },

        async observe(url, { onStatus, isComplete, ...options }) {
            await startConnection();

            const identifier = crypto.randomUUID().replaceAll("-", "");
            let resolve;
            let reject;
            const completion = new Promise((resolvePromise, rejectPromise) => {
                resolve = resolvePromise;
                reject = rejectPromise;
            });
            const observation = {
                identifier,
                resolve,
                reject,
                statusUrl: undefined,
                refreshing: false,
                refreshPending: false,
                onStatus,
                isComplete
            };
            pendingObservations.set(identifier, observation);

            try {
                const headers = new Headers(options.headers);
                headers.set(requestIdentifierHeader, identifier);
                const response = await fetch(url, { ...options, headers });
                if (!response.ok || !response.url) {
                    throw new Error("The Inventory Scan could not be submitted.");
                }

                observation.statusUrl = response.url;
                const status = await response.json();
                updateObservation(observation, status);
                if (observation.refreshPending &&
                    pendingObservations.has(identifier)) {
                    void refreshObservation(observation);
                }
            } catch (error) {
                pendingObservations.delete(identifier);
                reject(error);
            }

            return completion;
        }
    };
})();
