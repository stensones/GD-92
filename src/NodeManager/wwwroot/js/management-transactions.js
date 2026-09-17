window.managementTransactions = (() => {
    const requestIdentifierHeader = "X-GD92-Management-Request-Id";
    const pendingRequests = new Map();
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
        const request = pendingRequests.get(notification.requestIdentifier.replaceAll("-", ""));
        if (request === undefined) {
            return;
        }

        request.completed = true;
        void complete(request);
    });

    const reconcileOutstandingRequests = () => {
        for (const request of pendingRequests.values()) {
            void complete(request);
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
        }
    };
})();
