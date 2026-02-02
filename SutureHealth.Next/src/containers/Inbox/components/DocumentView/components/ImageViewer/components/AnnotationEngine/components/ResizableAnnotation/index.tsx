import { useCallback, useMemo, useState, cloneElement } from 'react';
import { Rnd, ResizeEnable } from 'react-rnd';
import { useInboxActions } from '@containers/Inbox/localReducer';
import { Box } from 'suture-theme';
import { AnnotationItem, AnnotationType } from '@utils/zodModels';
import round from '@utils/round';
import { useSelector } from '@redux/hooks';
import { BASE_SCALE, BASE_SCALE_RECOVERY } from '@lib/constants';
import AnnotationHandle from './components/AnnotationHandle';
import BallIcon from './components/Ball';

type ResizableAnnotationProps = {
  children: JSX.Element;
  annotation: AnnotationItem;
  docWidth: number;
  docHeight: number;
};

type RndData = {
  x?: number;
  y?: number;
  height: number;
  width: number;
};

export default function ResizableAnnotation({
  children,
  annotation,
  docWidth,
  docHeight,
}: ResizableAnnotationProps) {
  const { updateAnnotationItem } = useInboxActions();
  const isOpen = useSelector((state) => state.inbox.isEditing);
  const scale = useSelector((state) => state.inbox.scale);
  const [isResizing, setIsResizing] = useState<boolean>(false);
  const [dragTime, setDragTime] = useState<number>(0);
  const maxH =
    annotation.annotationTypeId === AnnotationType.VisibleSignature ? 52 : 400;
  const enableResizing: ResizeEnable = useMemo(() => {
    switch (annotation.annotationTypeId) {
      case AnnotationType.DateSigned:
      case AnnotationType.CheckBox:
        return false;

      case AnnotationType.VisibleSignature:
        return { left: true, right: true };

      default:
        return true;
    }
  }, [annotation]);

  const renderResizeHandlers = useCallback(() => {
    const { value, annotationTypeId } = annotation;
    const color =
      !value && annotationTypeId === AnnotationType.TextArea
        ? '#C22C27'
        : '#2AADFA';

    return (
      <>
        <Box position="absolute" top={-1} left={-1}>
          <BallIcon color={color} />
        </Box>
        <Box position="absolute" bottom={-1} right={-1}>
          <BallIcon color={color} />
        </Box>
        <Box position="absolute" bottom={-1} left={-1}>
          <BallIcon color={color} />
        </Box>
      </>
    );
  }, [annotation]);

  const saveAnnotation = (data: RndData) => {
    let left = data.x ?? annotation.left;
    let top = data.y ?? annotation.top;

    left = left < 0 ? 0 : left;
    top = top < 0 ? 0 : top;
    const updatedAnnotation = {
      ...annotation,
      left,
      top,
      right: left + data.width,
      bottom: top + data.height,
    };

    updateAnnotationItem(updatedAnnotation);
  };

  return (
    <Rnd
      disableDragging={!isOpen}
      size={{
        width: round((annotation.right - annotation.left) * BASE_SCALE, 0),
        height: round((annotation.bottom - annotation.top) * BASE_SCALE, 0),
      }}
      position={{
        x: round(
          (annotation.right > docWidth
            ? docWidth - (annotation.right - annotation.left)
            : annotation.left) * BASE_SCALE,
          0
        ),
        y: round(
          (annotation.bottom > docHeight
            ? docHeight - (annotation.bottom - annotation.top)
            : annotation.top) * BASE_SCALE,
          0
        ),
      }}
      onResizeStart={() => {
        setIsResizing(true);
      }}
      onDragStart={() => {
        setDragTime(new Date().getTime());
      }}
      onDragStop={(_, data) => {
        saveAnnotation({
          x: round(data.x * BASE_SCALE_RECOVERY, 0),
          y: round(data.y * BASE_SCALE_RECOVERY, 0),
          width: round(data.node.clientWidth * BASE_SCALE_RECOVERY, 0),
          height: round(data.node.clientHeight * BASE_SCALE_RECOVERY, 0),
        });

        setDragTime((prev) => new Date().getTime() - prev);
      }}
      onResizeStop={(_, direction, ref, delta, position) => {
        setIsResizing(false);
        saveAnnotation({
          width: round(parseInt(ref.style.width, 10) * BASE_SCALE_RECOVERY, 0),
          height: round(
            parseInt(ref.style.height, 10) * BASE_SCALE_RECOVERY,
            0
          ),
          x: position.x,
          y: position.y,
        });
      }}
      maxconstraints={[800, maxH]}
      minconstraints={[100, 52]}
      className="panzoom-exclude"
      enableResizing={enableResizing}
      bounds="parent"
      scale={scale}
    >
      <AnnotationHandle isResizing={isResizing} annotation={annotation}>
        {cloneElement(children, { isDragging: dragTime > 150 })}
      </AnnotationHandle>
      {enableResizing === true && renderResizeHandlers()}
    </Rnd>
  );
}
