import { ComponentFixture, TestBed } from '@angular/core/testing';

import { MigraManualComponent } from './migra-manual.component';

describe('MigraManualComponent', () => {
  let component: MigraManualComponent;
  let fixture: ComponentFixture<MigraManualComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      declarations: [MigraManualComponent]
    });
    fixture = TestBed.createComponent(MigraManualComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
