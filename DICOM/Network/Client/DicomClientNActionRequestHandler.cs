// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the N-ACTION request immediately.
    /// </summary>
    /// <param name="request">N-ACTION request subject to handling.</param>
    /// <returns>Response from handling the N-ACTION <paramref name="request"/>.</returns>
    public delegate Task<DicomNActionResponse> DicomClientNActionRequestHandler(DicomNActionRequest request);
}
