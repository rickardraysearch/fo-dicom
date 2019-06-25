// Copyright (c) 2012-2019 fo-dicom contributors.
// Licensed under the Microsoft Public License (MS-PL).

using System.Threading.Tasks;

namespace Dicom.Network.Client
{
    /// <summary>
    /// Delegate for client handling the N-DELETE request immediately. This is used when the DicomClient serves as an SCP for
    /// some SOP Classes in an association. See http://dicom.nema.org/medical/dicom/current/output/chtml/part07/sect_D.3.3.4.html
    /// for details on the SCP/SCU Role Selection.
    /// </summary>
    /// <param name="request">N-DELETE request subject to handling.</param>
    /// <returns>Response from handling the N-DELETE <paramref name="request"/>.</returns>
    public delegate Task<DicomNDeleteResponse> DicomClientNDeleteRequestHandler(DicomNDeleteRequest request);
}
