// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the N-EVENT-REPORT request immediately.
    /// </summary>
    /// <param name="request">N-EVENT-REPORT request subject to handling.</param>
    /// <returns>Response from handling the N-EVENT-REPORT <paramref name="request"/>.</returns>
    public delegate Task<DicomNEventReportResponse> DicomClientNEventReportRequestHandler(DicomNEventReportRequest request);
}
