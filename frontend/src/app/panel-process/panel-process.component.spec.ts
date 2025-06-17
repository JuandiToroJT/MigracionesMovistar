import { ComponentFixture, TestBed } from '@angular/core/testing';

import { PanelProcessComponent } from './panel-process.component';

describe('PanelProcessComponent', () => {
  let component: PanelProcessComponent;
  let fixture: ComponentFixture<PanelProcessComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [PanelProcessComponent]
    });
    fixture = TestBed.createComponent(PanelProcessComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
