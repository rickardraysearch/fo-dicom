// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dicom.Log;
using Dicom.Network.Client.EventArguments;
using Dicom.Network.Client.States;
using Dicom.Network.Client.Tasks;

namespace Dicom.Network.Client
{
    public interface IDicomClient
    {
        /// <summary>
        /// Gets or sets the logger that will be used by this DicomClient
        /// </summary>
        Logger Logger { get; set; }

        /// <summary>
        /// Gets or sets options to control behavior of <see cref="DicomService"/> base class.
        /// </summary>
        DicomServiceOptions Options { get; set; }

        /// <summary>
        /// Gets or sets additional presentation contexts to negotiate with association.
        /// </summary>
        List<DicomPresentationContext> AdditionalPresentationContexts { get; set; }

        /// <summary>
        /// Gets or sets the fallback encoding.
        /// </summary>
        Encoding FallbackEncoding { get; set; }

        /// <summary>
        /// Gets or sets the timeout (in ms) to wait for an association response after sending an association request
        /// </summary>
        int AssociationRequestTimeoutInMs { get; }

        /// <summary>
        /// Gets or sets the timeout (in ms) to wait for an association release response after sending an association release request
        /// </summary>
        int AssociationReleaseTimeoutInMs { get; }

        /// <summary>
        /// Gets or sets the timeout (in ms) that associations need to be held open after all requests have been processed
        /// </summary>
        int AssociationLingerTimeoutInMs { get; }

        /// <summary>
        /// Gets or sets the handler of a client C-STORE request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientCStoreRequestHandler OnCStoreRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client C-ECHO request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientCEchoRequestHandler OnCEchoRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client C-FIND request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientCFindRequestHandler OnCFindRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client N-GET request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientNGetRequestHandler OnNGetRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client N-SET request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientNSetRequestHandler OnNSetRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client N-ACTION request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientNActionRequestHandler OnNActionRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client N-CREATE request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientNCreateRequestHandler OnNCreateRequest { get; set; }

        /// <summary>
        /// Gets or sets the handler of a client N-DELETE request used in the special case when the DicomClient serves as SCP.
        /// </summary>
        DicomClientNDeleteRequestHandler OnNDeleteRequest { get; set; }

        /// <summary>
        /// Representation of the DICOM association accepted event.
        /// </summary>
        event EventHandler<EventArguments.AssociationAcceptedEventArgs> AssociationAccepted;

        /// <summary>
        /// Representation of the DICOM association rejected event.
        /// </summary>
        event EventHandler<EventArguments.AssociationRejectedEventArgs> AssociationRejected;

        /// <summary>
        /// Representation of the DICOM association released event.
        /// </summary>
        event EventHandler AssociationReleased;

        /// <summary>
        /// Whenever the DICOM client changes state, an event will be emitted containing the old state and the new state.
        /// </summary>
        event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Set negotiation asynchronous operations.
        /// </summary>
        /// <param name="invoked">Asynchronous operations invoked.</param>
        /// <param name="performed">Asynchronous operations performed.</param>
        void NegotiateAsyncOps(int invoked = 0, int performed = 0);

        /// <summary>
        /// Enqueues a new DICOM request for execution.
        /// </summary>
        /// <param name="dicomRequest">The DICOM request to send</param>
        Task AddRequestAsync(DicomRequest dicomRequest);

        /// <summary>
        /// Enqueues new DICOM requests for execution.
        /// When you have many requests, this method is recommended over calling <see cref="AddRequestAsync"/> multiple times.
        /// </summary>
        /// <param name="dicomRequests">The DICOM requests to send</param>
        Task AddRequestsAsync(IEnumerable<DicomRequest> dicomRequests);

        /// <summary>
        /// Sends existing requests to DICOM service. Note that subsequent calls, when the DICOM client is already sending its requests, will be completely ignored.
        /// If you want to cancel the process, be sure to cancel the cancellation token that was passed into the first call.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token that can abort the send process if necessary</param>
        /// <param name="cancellationMode">The cancellation mode that determines the cancellation behavior</param>
        Task SendAsync(CancellationToken cancellationToken = default(CancellationToken),
            DicomClientCancellationMode cancellationMode = DicomClientCancellationMode.ImmediatelyReleaseAssociation);
    }

    public class DicomClient : IDicomClient
    {
        private readonly SemaphoreSlim _transitionLock = new SemaphoreSlim(1, 1);
        private IDicomClientState State { get; set; }

        internal ConcurrentQueue<StrongBox<DicomRequest>> QueuedRequests { get; }
        internal int AsyncInvoked { get; private set; }
        internal int AsyncPerformed { get; private set; }

        public string Host { get; }
        public int Port { get; }
        public bool UseTls { get; }
        public string CallingAe { get; }
        public string CalledAe { get; }
        public int AssociationRequestTimeoutInMs { get; set; }
        public int AssociationReleaseTimeoutInMs { get; set; }
        public int AssociationLingerTimeoutInMs { get; set; }
        public bool IsSendRequired => State is DicomClientIdleState && QueuedRequests.Any();
        public Logger Logger { get; set; } = LogManager.GetLogger("Dicom.Network");
        public DicomServiceOptions Options { get; set; }
        public List<DicomPresentationContext> AdditionalPresentationContexts { get; set; }
        public Encoding FallbackEncoding { get; set; }
        public DicomClientCStoreRequestHandler OnCStoreRequest { get; set; }
        public DicomClientCEchoRequestHandler OnCEchoRequest { get; set; }
        public DicomClientCFindRequestHandler OnCFindRequest { get; set; }
        public DicomClientNEventReportRequestHandler OnNEventReportRequest { get; set; }
        public DicomClientNGetRequestHandler OnNGetRequest { get; set; }
        public DicomClientNSetRequestHandler OnNSetRequest { get; set; }
        public DicomClientNActionRequestHandler OnNActionRequest { get; set; }
        public DicomClientNCreateRequestHandler OnNCreateRequest { get; set; }
        public DicomClientNDeleteRequestHandler OnNDeleteRequest { get; set; }

        public event EventHandler<EventArguments.AssociationAcceptedEventArgs> AssociationAccepted;
        public event EventHandler<EventArguments.AssociationRejectedEventArgs> AssociationRejected;
        public event EventHandler AssociationReleased;
        public event EventHandler<StateChangedEventArgs> StateChanged;

        /// <summary>
        /// Initializes an instance of <see cref="DicomClient"/>.
        /// </summary>
        /// <param name="host">DICOM host.</param>
        /// <param name="port">Port.</param>
        /// <param name="useTls">True if TLS security should be enabled, false otherwise.</param>
        /// <param name="callingAe">Calling Application Entity Title.</param>
        /// <param name="calledAe">Called Application Entity Title.</param>
        /// <param name="associationRequestTimeoutInMs">Timeout in milliseconds for establishing association.</param>
        /// <param name="associationReleaseTimeoutInMs">Timeout in milliseconds to break off association</param>
        /// <param name="associationLingerTimeoutInMs">Timeout in milliseconds to keep open association after all requests have been processed.</param>
        public DicomClient(string host, int port, bool useTls, string callingAe, string calledAe,
            int associationRequestTimeoutInMs = DicomClientDefaults.DefaultAssociationRequestTimeoutInMs,
            int associationReleaseTimeoutInMs = DicomClientDefaults.DefaultAssociationReleaseTimeoutInMs,
            int associationLingerTimeoutInMs = DicomClientDefaults.DefaultAssociationLingerInMs)
        {
            Host = host;
            Port = port;
            UseTls = useTls;
            CallingAe = callingAe;
            CalledAe = calledAe;
            AssociationRequestTimeoutInMs = associationRequestTimeoutInMs;
            AssociationReleaseTimeoutInMs = associationReleaseTimeoutInMs;
            AssociationLingerTimeoutInMs = associationLingerTimeoutInMs;
            QueuedRequests = new ConcurrentQueue<StrongBox<DicomRequest>>();
            AdditionalPresentationContexts = new List<DicomPresentationContext>();
            AsyncInvoked = 1;
            AsyncPerformed = 1;
            State = new DicomClientIdleState(this);
        }

        private async Task ExecuteWithinTransitionLock(Func<Task> task)
        {
            await _transitionLock.WaitAsync().ConfigureAwait(false);
            try
            {
                await task().ConfigureAwait(false);
            }
            finally
            {
                _transitionLock.Release();
            }
        }

        internal async Task<IDicomClientState> Transition(IDicomClientState newState, DicomClientCancellation cancellation)
        {
            Task InternalTransition()
            {
                var oldState = State;

                Logger.Debug($"[{oldState}] --> [{newState}]");

                oldState.Dispose();

                State = newState ?? throw new ArgumentNullException(nameof(newState));

                StateChanged?.Invoke(this, new StateChangedEventArgs(oldState, newState));

                return CompletedTaskProvider.CompletedTask;
            }

            await ExecuteWithinTransitionLock(InternalTransition).ConfigureAwait(false);

            return await newState.GetNextStateAsync(cancellation).ConfigureAwait(false);
        }

        internal void NotifyAssociationAccepted(EventArguments.AssociationAcceptedEventArgs eventArgs)
            => AssociationAccepted?.Invoke(this, eventArgs);

        internal void NotifyAssociationRejected(EventArguments.AssociationRejectedEventArgs eventArgs)
            => AssociationRejected?.Invoke(this, eventArgs);

        internal void NotifyAssociationReleased()
            => AssociationReleased?.Invoke(this, EventArgs.Empty);

        internal Task OnSendQueueEmptyAsync()
            => State.OnSendQueueEmptyAsync();

        internal Task OnRequestCompletedAsync(DicomRequest request, DicomResponse response)
            => State.OnRequestCompletedAsync(request, response);

        internal Task OnReceiveAssociationAcceptAsync(DicomAssociation association)
            => ExecuteWithinTransitionLock(() => State.OnReceiveAssociationAcceptAsync(association));

        internal Task OnReceiveAssociationRejectAsync(DicomRejectResult result, DicomRejectSource source, DicomRejectReason reason)
            => ExecuteWithinTransitionLock(() => State.OnReceiveAssociationRejectAsync(result, source, reason));

        internal Task OnReceiveAssociationReleaseResponseAsync()
            => ExecuteWithinTransitionLock(() => State.OnReceiveAssociationReleaseResponseAsync());

        internal Task OnReceiveAbortAsync(DicomAbortSource source, DicomAbortReason reason)
            => ExecuteWithinTransitionLock(() => State.OnReceiveAbortAsync(source, reason));

        internal Task OnConnectionClosedAsync(Exception exception)
            => ExecuteWithinTransitionLock(() => State.OnConnectionClosedAsync(exception));

        internal async Task<DicomResponse> OnCStoreRequestAsync(DicomCStoreRequest request)
        {
            if (OnCStoreRequest == null)
                return new DicomCStoreResponse(request, DicomStatus.StorageStorageOutOfResources);

            return await OnCStoreRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomResponse> OnCEchoRequestAsync(DicomCEchoRequest request)
        {
            if (OnCEchoRequest == null)
                return new DicomCEchoResponse(request, DicomStatus.StorageStorageOutOfResources);

            return await OnCEchoRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomResponse> OnCFindRequestAsync(DicomCFindRequest request)
        {
            if (OnCFindRequest == null)
                return new DicomCFindResponse(request, DicomStatus.StorageStorageOutOfResources);

            return await OnCFindRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomNEventReportResponse> OnNEventReportRequestAsync(DicomNEventReportRequest request)
        {
            if (OnNGetRequest == null)
                return new DicomNEventReportResponse(request, DicomStatus.AttributeListError);

            return await OnNEventReportRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomNGetResponse> OnNGetRequestAsync(DicomNGetRequest request)
        {
            if (OnNGetRequest == null)
                return new DicomNGetResponse(request, DicomStatus.AttributeListError);

            return await OnNGetRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomNSetResponse> OnNSetRequestAsync(DicomNSetRequest request)
        {
            if (OnNSetRequest == null)
                return new DicomNSetResponse(request, DicomStatus.AttributeListError);

            return await OnNSetRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomNActionResponse> OnNActionRequestAsync(DicomNActionRequest request)
        {
            if (OnNActionRequest == null)
                return new DicomNActionResponse(request, DicomStatus.AttributeListError);

            return await OnNActionRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomNCreateResponse> OnNCreateRequestAsync(DicomNCreateRequest request)
        {
            if (OnNActionRequest == null)
                return new DicomNCreateResponse(request, DicomStatus.AttributeListError);

            return await OnNCreateRequest(request).ConfigureAwait(false);
        }

        internal async Task<DicomNDeleteResponse> OnNDeleteRequestAsync(DicomNDeleteRequest request)
        {
            if (OnNActionRequest == null)
                return new DicomNDeleteResponse(request, DicomStatus.AttributeListError);

            return await OnNDeleteRequest(request).ConfigureAwait(false);
        }


        public void NegotiateAsyncOps(int invoked = 0, int performed = 0)
        {
            AsyncInvoked = invoked;
            AsyncPerformed = performed;
        }

        public Task AddRequestAsync(DicomRequest dicomRequest)
            => State.AddRequestAsync(dicomRequest);

        public async Task AddRequestsAsync(IEnumerable<DicomRequest> dicomRequests)
        {
            if (dicomRequests == null)
                return;

            var requests = dicomRequests.ToList();

            if (!requests.Any())
                return;

            foreach (var request in requests)
                await State.AddRequestAsync(request).ConfigureAwait(false);
        }

        public async Task SendAsync(CancellationToken cancellationToken = default(CancellationToken),
            DicomClientCancellationMode cancellationMode = DicomClientCancellationMode.ImmediatelyReleaseAssociation)
        {
            var cancellation = new DicomClientCancellation(cancellationToken, cancellationMode);
            await State.SendAsync(cancellation).ConfigureAwait(false);
        }
    }
}
