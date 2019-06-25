// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the C-STORE request immediately.
    /// </summary>
    /// <param name="request">C-STORE request subject to handling.</param>
    /// <returns>Response from handling the C-STORE <paramref name="request"/>.</returns>
    public delegate Task<DicomCStoreResponse> DicomClientCStoreRequestHandler(DicomCStoreRequest request);

    /// <summary>
    /// Delegate for client handling the C-ECHO request immediately.
    /// </summary>
    /// <param name="request">C-ECHO request subject to handling.</param>
    /// <returns>Response from handling the C-ECHO <paramref name="request"/>.</returns>
    public delegate Task<DicomCEchoResponse> DicomClientCEchoRequestHandler(DicomCEchoRequest request);

    /// <summary>
    /// Delegate for client handling the C-FIND request immediately.
    /// </summary>
    /// <param name="request">C-FIND request subject to handling.</param>
    /// <returns>Response from handling the C-FIND <paramref name="request"/>.</returns>
    public delegate Task<DicomCFindResponse> DicomClientCFindRequestHandler(DicomCFindRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate Task<DicomNEventReportResponse> DicomClientNEventReportRequestHandler(DicomNEventReportRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate Task<DicomNGetResponse> DicomClientNGetRequestHandler(DicomNGetRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate Task<DicomNSetResponse> DicomClientNSetRequestHandler(DicomNSetRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate Task<DicomNActionResponse> DicomClientNActionRequestHandler(DicomNActionRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate Task<DicomNCreateResponse> DicomClientNCreateRequestHandler(DicomNCreateRequest request);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    public delegate Task<DicomNDeleteResponse> DicomClientNDeleteRequestHandler(DicomNDeleteRequest request);

}
