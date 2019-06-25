// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the N-DELETE request immediately.
    /// </summary>
    /// <param name="request">N-DELETE request subject to handling.</param>
    /// <returns>Response from handling the N-DELETE <paramref name="request"/>.</returns>
    public delegate Task<DicomNDeleteResponse> DicomClientNDeleteRequestHandler(DicomNDeleteRequest request);
}
