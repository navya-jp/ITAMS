import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { DecommissionService } from './decommission.service';

@Component({
  selector: 'app-decommission',
  imports: [CommonModule, FormsModule],
  templateUrl: './decommission.html'
})
export class Decommission implements OnInit {
  activeTab: 'eol' | 'pending' | 'all' | 'archive' = 'eol';

  eolCandidates: any[] = [];
  pendingRequests: any[] = [];
  allRequests: any[] = [];
  archives: any[] = [];

  loading = false;
  error = '';
  success = '';

  filterStatus = '';

  // Initiate modal
  showInitiateModal = false;
  selectedAsset: any = null;
  initiateForm = { assetId: 0, reason: '', disposalMethod: '', notes: '' };
  disposalMethods = ['Scrap', 'Donate', 'Return to Vendor', 'Write-off'];

  // Approve/Reject modal
  showApproveModal = false;
  selectedRequest: any = null;
  approveComments = '';
  rejectReason = '';
  modalAction: 'approve' | 'reject' = 'approve';

  // Archive detail
  selectedArchive: any = null;

  constructor(private svc: DecommissionService) {}

  ngOnInit() {
    this.loadEol();
    this.loadPending();
    this.loadAll();
    this.loadArchive();
  }

  loadEol() {
    this.svc.getEolCandidates().subscribe({ next: d => this.eolCandidates = d, error: () => {} });
  }

  loadPending() {
    this.svc.getMyPendingRequests().subscribe({ next: d => this.pendingRequests = d, error: () => {} });
  }

  loadAll() {
    const f = this.filterStatus ? { status: this.filterStatus } : undefined;
    this.svc.getRequests(f).subscribe({ next: d => this.allRequests = d.requests ?? d, error: () => {} });
  }

  loadArchive() {
    this.svc.getArchive().subscribe({ next: d => this.archives = d.archives ?? d, error: () => {} });
  }

  openInitiate(asset: any) {
    this.selectedAsset = asset;
    this.initiateForm = { assetId: asset.id, reason: '', disposalMethod: '', notes: '' };
    this.showInitiateModal = true;
    this.clearMessages();
  }

  submitInitiate() {
    if (!this.initiateForm.reason || !this.initiateForm.disposalMethod) return;
    this.loading = true;
    this.svc.initiateDecommission(this.initiateForm).subscribe({
      next: () => {
        this.success = 'Decommission request initiated successfully';
        this.showInitiateModal = false;
        this.loading = false;
        this.loadEol(); this.loadPending(); this.loadAll();
      },
      error: (e) => { this.error = e.error?.message || 'Failed'; this.loading = false; }
    });
  }

  openApprove(req: any) {
    this.selectedRequest = req;
    this.approveComments = '';
    this.rejectReason = '';
    this.modalAction = 'approve';
    this.showApproveModal = true;
    this.clearMessages();
  }

  openReject(req: any) {
    this.selectedRequest = req;
    this.approveComments = '';
    this.rejectReason = '';
    this.modalAction = 'reject';
    this.showApproveModal = true;
    this.clearMessages();
  }

  submitApproveReject() {
    if (!this.selectedRequest) return;
    this.loading = true;
    const obs = this.modalAction === 'approve'
      ? this.svc.approveRequest(this.selectedRequest.id, this.approveComments)
      : this.svc.rejectRequest(this.selectedRequest.id, this.rejectReason);

    obs.subscribe({
      next: (r) => {
        this.success = r.message || 'Done';
        this.showApproveModal = false;
        this.loading = false;
        this.loadPending(); this.loadAll(); this.loadArchive();
      },
      error: (e) => { this.error = e.error?.message || 'Failed'; this.loading = false; }
    });
  }

  getStatusClass(status: string): string {
    return { PENDING: 'warning', APPROVED: 'success', REJECTED: 'danger' }[status] ?? 'secondary';
  }

  clearMessages() { this.error = ''; this.success = ''; }

  parseSnapshot(json: string): any {
    try { return JSON.parse(json); } catch { return {}; }
  }

  parseChain(json: string): any[] {
    try { return JSON.parse(json); } catch { return []; }
  }
}
