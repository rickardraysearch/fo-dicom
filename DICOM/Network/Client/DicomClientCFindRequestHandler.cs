// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the C-FIND request immediately.
    /// </summary>
    /// <param name="request">C-FIND request subject to handling.</param>
    /// <returns>Response from handling the C-FIND <paramref name="request"/>.</returns>
    public delegate Task<DicomCFindResponse> DicomClientCFindRequestHandler(DicomCFindRequest request);
}
